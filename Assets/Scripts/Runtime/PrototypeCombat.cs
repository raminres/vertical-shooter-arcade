using System;
using RichHaptic;
using UnityEngine;

namespace VerticalShooter.Gameplay
{
    public sealed class PrototypeCombat : MonoBehaviour
    {
        [SerializeField] private FlightSettings settings;
        [SerializeField] private PrototypeFlight flight;
        [SerializeField] private PrototypeEffects effects;
        [SerializeField] private Transform[] projectiles;
        [SerializeField] private Transform[] enemyProjectiles;
        [SerializeField] private float muzzleOffset = .85f;

        private Vector3[] enemyPrevious;
        private int[] enemyGeneration;
        private bool[] enemyWasActive;
        private float fireTimer;
        private float[] enemyFireTimers;
        private int[] firingGeneration;
        private Vector3 playerPrevious;
        private float invulnerabilityRemaining;
        public event Action<int, int> PlayerHealthChanged;
        public int PlayerHealth { get; private set; }
        public int MaxPlayerHealth => Mathf.Max(1, settings.playerHitPoints);
        public bool IsDefeated => PlayerHealth <= 0;
        public int EnemyProjectileCapacity => enemyProjectiles.Length;
        public int ActiveEnemyProjectiles { get; private set; }
        private EnemyHapticFeedback hapticFeedback;
        public int ProjectileCapacity => projectiles.Length;
        public int ActiveProjectiles { get; private set; }

        private void OnEnable()
        {
            hapticFeedback = new EnemyHapticFeedback(Haptics.PlayPreset);
            flight.EnemyHit += hapticFeedback.OnEnemyHit;
            enemyFireTimers = new float[flight.Capacity];
            firingGeneration = new int[flight.Capacity];
            PlayerHealth = MaxPlayerHealth;
            invulnerabilityRemaining = 0f;
            foreach (var projectile in enemyProjectiles) projectile.gameObject.SetActive(false);
            ActiveEnemyProjectiles = 0;
            enemyPrevious = new Vector3[flight.Capacity];
            enemyGeneration = new int[flight.Capacity];
            enemyWasActive = new bool[flight.Capacity];
            foreach (var projectile in projectiles) projectile.gameObject.SetActive(false);
            ActiveProjectiles = 0;
            fireTimer = 0f;
        }

        private void OnDisable()
        {
            if (hapticFeedback != null) flight.EnemyHit -= hapticFeedback.OnEnemyHit;
        }

        public void CaptureEnemies()
        {
            playerPrevious = flight.Aircraft.localPosition;
            for (int i = 0; i < flight.Capacity; i++)
            {
                var enemy = flight.GetEnemy(i);
                enemyWasActive[i] = enemy.gameObject.activeSelf;
                enemyPrevious[i] = enemy.localPosition;
                enemyGeneration[i] = flight.GetEnemyGeneration(i);
            }
        }

        public void Simulate(float deltaTime)
        {
            if (deltaTime <= 0f || IsDefeated) return;
            invulnerabilityRemaining = Mathf.Max(0f, invulnerabilityRemaining - deltaTime);
            TickEnemyProjectiles(deltaTime);
            if (IsDefeated) return;
            TickEnemyFire(deltaTime);
            for (int p = 0; p < projectiles.Length; p++)
            {
                var projectile = projectiles[p];
                if (!projectile.gameObject.activeSelf) continue;
                Vector3 from = projectile.localPosition;
                Vector3 to = from + Vector3.forward * (settings.projectileSpeed * deltaTime);
                int hit = -1;
                float earliest = float.PositiveInfinity;
                for (int i = 0; i < flight.Capacity; i++)
                {
                    var enemy = flight.GetEnemy(i);
                    if (!enemy.gameObject.activeSelf) continue;
                    Vector3 current = enemy.localPosition;
                    Vector3 previous = enemyWasActive[i] && enemyGeneration[i] == flight.GetEnemyGeneration(i) ? enemyPrevious[i] : current;
                    if (CombatMath.TryHitTime(new Vector2(from.x, from.z), new Vector2(to.x, to.z),
                        new Vector2(previous.x, previous.z), new Vector2(current.x, current.z),
                        settings.enemyHitRadius + settings.projectileRadius, out float time) && time < earliest)
                    {
                        earliest = time;
                        hit = i;
                    }
                }
                if (hit >= 0)
                {
                    flight.DamageEnemy(hit, settings.projectileDamage);
                    Release(projectile);
                    continue;
                }
                projectile.localPosition = to;
                if (flight.FlightCamera.WorldToViewportPoint(projectile.position).y > 1.15f)
                    Release(projectile);
            }

            fireTimer -= deltaTime;
            if (fireTimer > 0f) return;
            TryFire();
            // A full pool skips this shot and resumes on the next firing interval.
            fireTimer = Mathf.Max(.01f, settings.fireInterval);
        }

        public bool TryFire()
        {
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].gameObject.activeSelf) continue;
                projectiles[i].localPosition = flight.Aircraft.localPosition + Vector3.forward * muzzleOffset;
                projectiles[i].gameObject.SetActive(true);
                ActiveProjectiles++;
                return true;
            }
            return false;
        }

        private void TickEnemyProjectiles(float deltaTime)
        {
            for (int i = 0; i < enemyProjectiles.Length; i++)
            {
                var projectile = enemyProjectiles[i];
                if (!projectile.gameObject.activeSelf) continue;
                Vector3 from = projectile.localPosition;
                Vector3 to = from + Vector3.back * (settings.enemyProjectileSpeed * deltaTime);
                Vector3 current = flight.Aircraft.localPosition;
                if (CombatMath.TryHitTime(new Vector2(from.x, from.z), new Vector2(to.x, to.z),
                    new Vector2(playerPrevious.x, playerPrevious.z), new Vector2(current.x, current.z),
                    settings.playerHitRadius + settings.enemyProjectileRadius, out _))
                {
                    DamagePlayer(settings.enemyProjectileDamage);
                    projectile.gameObject.SetActive(false);
                    ActiveEnemyProjectiles--;
                    continue;
                }
                projectile.localPosition = to;
                Vector3 viewport = flight.FlightCamera.WorldToViewportPoint(projectile.position);
                if (viewport.z <= 0f || viewport.y < -.15f)
                {
                    projectile.gameObject.SetActive(false);
                    ActiveEnemyProjectiles--;
                }
            }
        }

        private void TickEnemyFire(float deltaTime)
        {
            for (int i = 0; i < flight.Capacity; i++)
            {
                var enemy = flight.GetEnemy(i);
                if (!enemy.gameObject.activeSelf) continue;
                if (firingGeneration[i] != flight.GetEnemyGeneration(i))
                {
                    firingGeneration[i] = flight.GetEnemyGeneration(i);
                    enemyFireTimers[i] = settings.enemyFireInterval;
                }
                float viewportY = flight.FlightCamera.WorldToViewportPoint(enemy.position).y;
                if (viewportY < 0f || viewportY > 1f) continue;
                enemyFireTimers[i] -= deltaTime;
                if (enemyFireTimers[i] > 0f) continue;
                TryFireEnemy(i);
                // A full pool skips this shot until the next interval.
                enemyFireTimers[i] = Mathf.Max(.1f, settings.enemyFireInterval);
            }
        }

        public bool TryFireEnemy(int enemyIndex)
        {
            if (IsDefeated || enemyIndex < 0 || enemyIndex >= flight.Capacity ||
                !flight.GetEnemy(enemyIndex).gameObject.activeSelf) return false;
            for (int i = 0; i < enemyProjectiles.Length; i++)
            {
                if (enemyProjectiles[i].gameObject.activeSelf) continue;
                enemyProjectiles[i].localPosition = flight.GetEnemy(enemyIndex).localPosition + Vector3.back * .7f;
                enemyProjectiles[i].gameObject.SetActive(true);
                ActiveEnemyProjectiles++;
                return true;
            }
            return false;
        }

        public bool DamagePlayer(int damage)
        {
            if (damage <= 0 || IsDefeated || invulnerabilityRemaining > 0f) return false;
            PlayerHealth = Mathf.Max(0, PlayerHealth - damage);
            effects.PlayPlayerHit(flight.Aircraft.position);
            invulnerabilityRemaining = Mathf.Max(0f, settings.playerInvulnerability);
            PlayerHealthChanged?.Invoke(PlayerHealth, MaxPlayerHealth);
            return true;
        }

        private void Release(Transform projectile)
        {
            projectile.gameObject.SetActive(false);
            ActiveProjectiles--;
        }
    }
}

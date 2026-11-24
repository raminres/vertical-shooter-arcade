using UnityEngine;

namespace VerticalShooter.Gameplay
{
    public sealed class PrototypeCombat : MonoBehaviour
    {
        [SerializeField] private FlightSettings settings;
        [SerializeField] private PrototypeFlight flight;
        [SerializeField] private Transform[] projectiles;
        [SerializeField] private float muzzleOffset = .85f;

        private Vector3[] enemyPrevious;
        private int[] enemyGeneration;
        private bool[] enemyWasActive;
        private float fireTimer;
        public int ProjectileCapacity => projectiles.Length;
        public int ActiveProjectiles { get; private set; }

        private void OnEnable()
        {
            enemyPrevious = new Vector3[flight.Capacity];
            enemyGeneration = new int[flight.Capacity];
            enemyWasActive = new bool[flight.Capacity];
            foreach (var projectile in projectiles) projectile.gameObject.SetActive(false);
            ActiveProjectiles = 0;
            fireTimer = 0f;
        }

        public void CaptureEnemies()
        {
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
            if (deltaTime <= 0f) return;
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

        private void Release(Transform projectile)
        {
            projectile.gameObject.SetActive(false);
            ActiveProjectiles--;
        }
    }
}

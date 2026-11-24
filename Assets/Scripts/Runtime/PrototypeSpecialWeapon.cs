using System;
using UnityEngine;

namespace VerticalShooter.Gameplay
{
    public sealed class PrototypeSpecialWeapon : MonoBehaviour
    {
        [SerializeField] private SpecialWeaponSettings settings;
        [SerializeField] private FlightSettings flightSettings;
        [SerializeField] private PrototypeFlight flight;
        [SerializeField] private PrototypeCombat combat;
        [SerializeField] private Transform laser;
        [SerializeField] private Transform[] shots;
        private Vector3[] velocities, previousEnemies;
        private int[] generations;
        private bool[] wasActive, guided;
        private float laserRemaining, laserTick;
        public event Action ChargesChanged;
        public int Charges { get; private set; }
        public SpecialWeaponType Selected { get; private set; }
        public int ActiveShots { get; private set; }
        public int ShotCapacity => shots.Length;
        public bool LaserActive => laserRemaining > 0f;

        private void OnEnable()
        {
            Selected = SpecialWeaponSelection.Selected;
            Charges = Mathf.Max(1, settings.startingCharges);
            velocities = new Vector3[shots.Length];
            guided = new bool[shots.Length];
            previousEnemies = new Vector3[flight.Capacity];
            generations = new int[flight.Capacity];
            wasActive = new bool[flight.Capacity];
            foreach (var shot in shots) shot.gameObject.SetActive(false);
            laser.gameObject.SetActive(false);
            laserRemaining = laserTick = 0f;
            ActiveShots = 0;
        }

        public bool TryUse()
        {
            if (Charges <= 0 || combat.IsDefeated || Time.timeScale <= 0f) return false;
            if (Selected == SpecialWeaponType.Laser)
            {
                if (LaserActive) return false;
                laserRemaining = Mathf.Max(.1f, settings.laserDuration);
                laserTick = 0f;
                PositionLaser();
                laser.gameObject.SetActive(true);
            }
            else
            {
                int count = Selected == SpecialWeaponType.Spread ? Mathf.Max(1, settings.spreadCount) : Mathf.Max(1, settings.missileCount);
                if (shots.Length - ActiveShots < count) return false;
                int spawned = 0;
                for (int i = 0; i < shots.Length && spawned < count; i++)
                {
                    if (shots[i].gameObject.activeSelf) continue;
                    float angle = Selected == SpecialWeaponType.Spread && count > 1
                        ? -settings.spreadAngle * .5f + settings.spreadAngle * spawned / (count - 1) : (spawned - (count - 1) * .5f) * 8f;
                    velocities[i] = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * settings.projectileSpeed;
                    guided[i] = Selected == SpecialWeaponType.Missiles;
                    shots[i].localPosition = flight.Aircraft.localPosition + Vector3.forward * .8f;
                    shots[i].gameObject.SetActive(true);
                    ActiveShots++;
                    spawned++;
                }
            }
            Charges--;
            ChargesChanged?.Invoke();
            return true;
        }

        public void CaptureEnemies()
        {
            for (int i = 0; i < flight.Capacity; i++)
            {
                var enemy = flight.GetEnemy(i);
                wasActive[i] = enemy.gameObject.activeSelf;
                previousEnemies[i] = enemy.localPosition;
                generations[i] = flight.GetEnemyGeneration(i);
            }
        }

        public void Simulate(float deltaTime)
        {
            if (deltaTime <= 0f || combat.IsDefeated) return;
            if (LaserActive)
            {
                PositionLaser();
                laserTick -= deltaTime;
                if (laserTick <= 0f)
                {
                    for (int i = 0; i < flight.Capacity; i++)
                    {
                        var enemy = flight.GetEnemy(i);
                        if (!enemy.gameObject.activeSelf) continue;
                        Vector3 relative = enemy.localPosition - flight.Aircraft.localPosition;
                        if (relative.z >= 0f && relative.z <= settings.laserRange && Mathf.Abs(relative.x) <= settings.laserWidth * .5f + flightSettings.enemyHitRadius)
                            flight.DamageEnemy(i, settings.laserDamage);
                    }
                    laserTick = Mathf.Max(.01f, settings.laserTickInterval);
                }
                laserRemaining -= deltaTime;
                if (laserRemaining <= 0f) { laserRemaining = 0f; laser.gameObject.SetActive(false); ChargesChanged?.Invoke(); }
            }
            for (int i = 0; i < shots.Length; i++)
            {
                if (!shots[i].gameObject.activeSelf) continue;
                Vector3 from = shots[i].localPosition;
                if (guided[i])
                {
                    var target = FindTarget(from);
                    if (target != null)
                    {
                        Vector3 direction = target.localPosition - from;
                        direction.y = 0f;
                        if (direction.sqrMagnitude > .0001f)
                            velocities[i] = Vector3.RotateTowards(velocities[i], direction.normalized * settings.projectileSpeed, settings.missileTurnSpeed * deltaTime, 0f);
                    }
                }
                Vector3 to = from + velocities[i] * deltaTime;
                int hit = -1;
                float earliest = float.PositiveInfinity;
                for (int e = 0; e < flight.Capacity; e++)
                {
                    var enemy = flight.GetEnemy(e);
                    if (!enemy.gameObject.activeSelf) continue;
                    var current = enemy.localPosition;
                    var previous = wasActive[e] && generations[e] == flight.GetEnemyGeneration(e) ? previousEnemies[e] : current;
                    if (CombatMath.TryHitTime(new Vector2(from.x, from.z), new Vector2(to.x, to.z),
                        new Vector2(previous.x, previous.z), new Vector2(current.x, current.z),
                        settings.projectileRadius + flightSettings.enemyHitRadius, out float t) && t < earliest)
                    { earliest = t; hit = e; }
                }
                if (hit >= 0)
                {
                    flight.DamageEnemy(hit, settings.projectileDamage);
                    Release(i);
                    continue;
                }
                shots[i].localPosition = to;
                var view = flight.FlightCamera.WorldToViewportPoint(shots[i].position);
                if (view.z <= 0f || view.y > 1.15f || view.y < -.15f || view.x < -.15f || view.x > 1.15f) Release(i);
            }
        }

        private Transform FindTarget(Vector3 from)
        {
            Transform nearest = null;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < flight.Capacity; i++)
            {
                var enemy = flight.GetEnemy(i);
                if (!enemy.gameObject.activeSelf || enemy.localPosition.z < from.z) continue;
                var view = flight.FlightCamera.WorldToViewportPoint(enemy.position);
                if (view.z <= 0f || view.x < 0f || view.x > 1f || view.y < 0f || view.y > 1f) continue;
                float d = (enemy.localPosition - from).sqrMagnitude;
                if (d < distance) { distance = d; nearest = enemy; }
            }
            return nearest;
        }

        private void PositionLaser()
        {
            laser.localPosition = flight.Aircraft.localPosition + Vector3.forward * (settings.laserRange * .5f + .8f);
            laser.localScale = new Vector3(settings.laserWidth, .15f, settings.laserRange);
        }

        private void Release(int index)
        {
            shots[index].gameObject.SetActive(false);
            ActiveShots--;
        }
    }
}

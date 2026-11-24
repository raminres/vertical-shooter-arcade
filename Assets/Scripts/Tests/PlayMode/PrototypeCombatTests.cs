using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VerticalShooter.Gameplay;

namespace VerticalShooter.Tests
{
    public class PrototypeCombatTests
    {
        [UnityTest]
        public IEnumerator AutomaticFireDamagesAndDefeatsAnEnemy()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            var combat = Object.FindAnyObjectByType<PrototypeCombat>();
            int defeated = 0;
            flight.EnemyDefeated += () => defeated++;
            bool sawProjectile = false;
            bool sawDamage = false;
            for (int step = 0; step < 700 && defeated == 0; step++)
            {
                flight.Simulate(.02f, Vector2.zero, Vector2.zero);
                sawProjectile |= combat.ActiveProjectiles > 0;
                for (int i = 0; i < flight.Capacity; i++)
                    sawDamage |= flight.GetEnemy(i).gameObject.activeSelf && flight.GetEnemyHealth(i) == 1;
            }
            Assert.That(sawProjectile, Is.True);
            Assert.That(sawDamage, Is.True, "A light fighter should survive its first hit.");
            Assert.That(defeated, Is.GreaterThan(0), "A second hit should defeat a light fighter.");
            Assert.That(flight.GetComponentsInChildren<Transform>(true).Count(t => t.name.StartsWith("Projectile")), Is.EqualTo(combat.ProjectileCapacity));
        }

        [UnityTest]
        public IEnumerator EnemyHealthResetsWhenAPooledEnemyIsReused()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            for (int step = 0; step < 100 && flight.ActiveEnemies == 0; step++)
                flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            int index = Enumerable.Range(0, flight.Capacity).First(i => flight.GetEnemy(i).gameObject.activeSelf);
            int generation = flight.GetEnemyGeneration(index);
            Assert.That(flight.GetEnemyHealth(index), Is.EqualTo(2));
            Assert.That(flight.DamageEnemy(index, 1), Is.False);
            Assert.That(flight.GetEnemyHealth(index), Is.EqualTo(1));
            Assert.That(flight.DamageEnemy(index, 1), Is.True);
            Assert.That(flight.GetEnemy(index).gameObject.activeSelf, Is.False);
            for (int step = 0; step < 1200 && flight.GetEnemyGeneration(index) == generation; step++)
                flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            Assert.That(flight.GetEnemyGeneration(index), Is.GreaterThan(generation));
            Assert.That(flight.GetEnemyHealth(index), Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator FullProjectilePoolSkipsShotsAndReusesReleasedSlots()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            var combat = Object.FindAnyObjectByType<PrototypeCombat>();
            Assert.That(combat.ProjectileCapacity, Is.EqualTo(16));
            while (combat.ActiveProjectiles < combat.ProjectileCapacity)
                Assert.That(combat.TryFire(), Is.True);
            Assert.That(combat.TryFire(), Is.False);
            Assert.That(combat.ActiveProjectiles, Is.EqualTo(16));
            for (int step = 0; step < 200; step++) flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            Assert.That(combat.ActiveProjectiles, Is.LessThan(combat.ProjectileCapacity));
            Assert.That(combat.TryFire(), Is.True);
        }

        [UnityTest]
        public IEnumerator PausingStopsProjectilesAndAutomaticFire()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            var combat = Object.FindAnyObjectByType<PrototypeCombat>();
            Assert.That(combat.TryFire(), Is.True);
            var projectile = flight.GetComponentsInChildren<Transform>(true).First(t => t.name.StartsWith("Projectile") && t.gameObject.activeSelf);
            var oldTimeScale = Time.timeScale;
            try
            {
                Time.timeScale = 0f;
                var position = projectile.localPosition;
                int active = combat.ActiveProjectiles;
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(projectile.localPosition, Is.EqualTo(position));
                Assert.That(combat.ActiveProjectiles, Is.EqualTo(active));
            }
            finally { Time.timeScale = oldTimeScale; }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
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
        public IEnumerator EnemyDefeatAndPlayerDamageTriggerDistinctEffects()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            var combat = Object.FindAnyObjectByType<PrototypeCombat>();
            var systems = flight.GetComponentsInChildren<ParticleSystem>(true);
            var explosions = systems.Where(x => x.name.StartsWith("EnemyExplosion")).ToArray();
            var hits = systems.Where(x => x.name.StartsWith("PlayerHit")).ToArray();
            Assert.That(explosions.Length, Is.EqualTo(4));
            Assert.That(hits.Length, Is.EqualTo(2));
            Assert.That(explosions[0].main.startColor.colorMin.r, Is.GreaterThan(explosions[0].main.startColor.colorMin.b));
            Assert.That(hits[0].main.startColor.colorMin.b, Is.GreaterThan(hits[0].main.startColor.colorMin.r));
            for (int i = 0; i < 100 && flight.ActiveEnemies == 0; i++)
                flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            int enemy = Enumerable.Range(0, flight.Capacity).First(i => flight.GetEnemy(i).gameObject.activeSelf);
            Vector3 enemyPosition = flight.GetEnemy(enemy).position;
            Assert.That(flight.DamageEnemy(enemy, 1), Is.False);
            Assert.That(explosions.Any(x => x.isPlaying), Is.False);
            Assert.That(flight.DamageEnemy(enemy, 1), Is.True);
            var explosion = explosions.Single(x => x.isPlaying);
            Assert.That(Vector3.Distance(explosion.transform.position, enemyPosition), Is.LessThan(.01f));
            Assert.That(combat.DamagePlayer(1), Is.True);
            var hit = hits.Single(x => x.isPlaying);
            Assert.That(Vector3.Distance(hit.transform.position, flight.Aircraft.position), Is.LessThan(.01f));
            Assert.That(combat.DamagePlayer(1), Is.False);
            Assert.That(hits.Count(x => x.isPlaying), Is.EqualTo(1), "Invulnerable hits do not create another effect.");
            yield return null;
            Assert.That(explosion.particleCount, Is.GreaterThan(0));
            Assert.That(hit.particleCount, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator ImpactEffectsSkipWhenBusyAndReuseExistingSystems()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            var effects = flight.GetComponent<PrototypeEffects>();
            int count = flight.GetComponentsInChildren<ParticleSystem>(true).Length;
            int objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            for (int i = 0; i < 4; i++) Assert.That(effects.PlayEnemyExplosion(Vector3.zero), Is.True);
            Assert.That(effects.PlayEnemyExplosion(Vector3.zero), Is.False);
            for (int i = 0; i < 2; i++) Assert.That(effects.PlayPlayerHit(Vector3.zero), Is.True);
            Assert.That(effects.PlayPlayerHit(Vector3.zero), Is.False);
            yield return new WaitForSeconds(1f);
            Assert.That(effects.PlayEnemyExplosion(Vector3.one), Is.True);
            Assert.That(effects.PlayPlayerHit(Vector3.one), Is.True);
            Assert.That(flight.GetComponentsInChildren<ParticleSystem>(true).Length, Is.EqualTo(count));
            Assert.That(Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, Is.EqualTo(objects));
        }

        [UnityTest]
        public IEnumerator EnemyProjectilesDamagePlayerAndRespectInvulnerability()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            var combat = Object.FindAnyObjectByType<PrototypeCombat>();
            for (int i = 0; i < 100 && flight.ActiveEnemies == 0; i++)
                flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            int enemy = Enumerable.Range(0, flight.Capacity).First(i => flight.GetEnemy(i).gameObject.activeSelf);
            flight.GetEnemy(enemy).localPosition = flight.Aircraft.localPosition + Vector3.forward * 1.4f;
            Assert.That(combat.PlayerHealth, Is.EqualTo(10));
            combat.CaptureEnemies();
            Assert.That(combat.TryFireEnemy(enemy), Is.True);
            combat.Simulate(.11f);
            Assert.That(combat.PlayerHealth, Is.EqualTo(9));
            combat.CaptureEnemies();
            Assert.That(combat.TryFireEnemy(enemy), Is.True);
            combat.Simulate(.11f);
            Assert.That(combat.PlayerHealth, Is.EqualTo(9), "A second hit during invulnerability must not remove health.");
            combat.CaptureEnemies();
            combat.Simulate(1.01f);
            combat.CaptureEnemies();
            Assert.That(combat.TryFireEnemy(enemy), Is.True);
            combat.Simulate(.11f);
            Assert.That(combat.PlayerHealth, Is.EqualTo(8));
        }

        [UnityTest]
        public IEnumerator VisibleEnemyFiresAutomatically()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            var combat = Object.FindAnyObjectByType<PrototypeCombat>();
            for (int i = 0; i < 100 && flight.ActiveEnemies == 0; i++)
                flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            int enemy = Enumerable.Range(0, flight.Capacity).First(i => flight.GetEnemy(i).gameObject.activeSelf);
            flight.GetEnemy(enemy).localPosition = flight.Aircraft.localPosition + new Vector3(1.5f, 0f, 9f);
            for (int i = 0; i < 150 && combat.ActiveEnemyProjectiles == 0; i++)
                flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            Assert.That(combat.ActiveEnemyProjectiles, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator EnemyProjectilePoolSkipsAndReusesSlots()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            var combat = Object.FindAnyObjectByType<PrototypeCombat>();
            for (int i = 0; i < 100 && flight.ActiveEnemies == 0; i++)
                flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            int enemy = Enumerable.Range(0, flight.Capacity).First(i => flight.GetEnemy(i).gameObject.activeSelf);
            flight.GetEnemy(enemy).localPosition += Vector3.right * 100f;
            int startingObjectCount = flight.GetComponentsInChildren<Transform>(true).Length;
            for (int i = 0; i < combat.EnemyProjectileCapacity; i++) Assert.That(combat.TryFireEnemy(enemy), Is.True);
            Assert.That(combat.TryFireEnemy(enemy), Is.False);
            Assert.That(combat.ActiveEnemyProjectiles, Is.EqualTo(combat.EnemyProjectileCapacity));
            combat.CaptureEnemies();
            combat.Simulate(10f);
            Assert.That(combat.ActiveEnemyProjectiles, Is.LessThan(combat.EnemyProjectileCapacity));
            Assert.That(combat.TryFireEnemy(enemy), Is.True);
            Assert.That(flight.GetComponentsInChildren<Transform>(true).Length, Is.EqualTo(startingObjectCount));
        }

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
            // Keep automatic fire away from the center lane while checking health on reuse.
            flight.Aircraft.position += Vector3.right * 20f;
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
        public IEnumerator ValidHitsAndKillsEmitDistinctFeedbackEvents()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            var hits = new List<bool>();
            flight.EnemyHit += hits.Add;
            for (int step = 0; step < 100 && flight.ActiveEnemies == 0; step++)
                flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            int index = Enumerable.Range(0, flight.Capacity).First(i => flight.GetEnemy(i).gameObject.activeSelf);
            Assert.That(flight.DamageEnemy(index, 0), Is.False);
            Assert.That(flight.DamageEnemy(-1, 1), Is.False);
            Assert.That(hits, Is.Empty);
            Assert.That(flight.DamageEnemy(index, 1), Is.False);
            Assert.That(flight.DamageEnemy(index, 1), Is.True);
            Assert.That(flight.DamageEnemy(index, 1), Is.False);
            Assert.That(hits, Is.EqualTo(new[] { false, true }));
        }

        [UnityTest]
        public IEnumerator PlaneCameraAndEnemyTweensMoveAndReturnToRest()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            var camera = flight.FlightCamera;
            var plane = flight.Aircraft.Find("Visual");
            float initialFov = camera.fieldOfView;
            Quaternion initialRotation = plane.localRotation;
            int initialObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            flight.Simulate(.02f, Vector2.right, Vector2.zero);
            Assert.That(camera.fieldOfView, Is.GreaterThan(initialFov));
            Assert.That(camera.fieldOfView, Is.LessThan(initialFov + 1.5f));
            Assert.That(Quaternion.Angle(initialRotation, plane.localRotation), Is.GreaterThan(0f));
            for (int i = 0; i < 25; i++) flight.Simulate(.02f, Vector2.right, Vector2.zero);
            Assert.That(camera.fieldOfView, Is.EqualTo(initialFov + 1.5f).Within(.01f));
            for (int i = 0; i < 25; i++) flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            Assert.That(camera.fieldOfView, Is.EqualTo(initialFov).Within(.01f));
            Assert.That(Quaternion.Angle(initialRotation, plane.localRotation), Is.LessThan(.01f));
            for (int i = 0; i < 100 && flight.ActiveEnemies == 0; i++) flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            int index = Enumerable.Range(0, flight.Capacity).First(i => flight.GetEnemy(i).gameObject.activeSelf);
            var enemy = flight.GetEnemy(index);
            var baseScale = enemy.localScale;
            flight.DamageEnemy(index, 1);
            Assert.That(enemy.localScale.x, Is.EqualTo(baseScale.x * .8f).Within(.001f));
            flight.Simulate(.03f, Vector2.zero, Vector2.zero);
            Assert.That(enemy.localScale.x, Is.GreaterThan(baseScale.x * .8f));
            for (int i = 0; i < 10; i++) flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            Assert.That(enemy.localScale.x, Is.EqualTo(baseScale.x).Within(.001f));
            Assert.That(Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, Is.EqualTo(initialObjects));
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
            for (int i = 0; i < 100 && flight.ActiveEnemies == 0; i++)
                flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            int enemy = Enumerable.Range(0, flight.Capacity).First(i => flight.GetEnemy(i).gameObject.activeSelf);
            Assert.That(combat.TryFireEnemy(enemy), Is.True);
            var enemyProjectile = flight.GetComponentsInChildren<Transform>(true).First(t => t.name.StartsWith("EnemyProjectile") && t.gameObject.activeSelf);
            Assert.That(combat.TryFire(), Is.True);
            var projectile = flight.GetComponentsInChildren<Transform>(true).First(t => t.name.StartsWith("Projectile") && t.gameObject.activeSelf);
            var oldTimeScale = Time.timeScale;
            try
            {
                Time.timeScale = 0f;
                var position = projectile.localPosition;
                var enemyPosition = enemyProjectile.localPosition;
                int enemyActive = combat.ActiveEnemyProjectiles;
                int active = combat.ActiveProjectiles;
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(projectile.localPosition, Is.EqualTo(position));
                Assert.That(combat.ActiveProjectiles, Is.EqualTo(active));
                Assert.That(enemyProjectile.localPosition, Is.EqualTo(enemyPosition));
                Assert.That(combat.ActiveEnemyProjectiles, Is.EqualTo(enemyActive));
            }
            finally { Time.timeScale = oldTimeScale; }
        }
    }
}

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

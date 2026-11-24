using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VerticalShooter.Gameplay;

namespace VerticalShooter.Tests
{
    public class SpecialWeaponTests
    {
        [UnityTest]
        public IEnumerator SpecialsConsumeChargesAndResetOnRetry()
        {
            foreach (SpecialWeaponType choice in new[] { SpecialWeaponType.Laser, SpecialWeaponType.Spread, SpecialWeaponType.Missiles })
            {
                SpecialWeaponSelection.Selected = choice;
                yield return SceneManager.LoadSceneAsync("Gameplay");
                yield return null;
                var special = Object.FindAnyObjectByType<PrototypeSpecialWeapon>();
                var flight = Object.FindAnyObjectByType<PrototypeFlight>();
                var combat = Object.FindAnyObjectByType<PrototypeCombat>();
                Assert.That(special.Selected, Is.EqualTo(choice));
                Assert.That(special.Charges, Is.EqualTo(2));
                Time.timeScale = 0;
                Assert.That(special.TryUse(), Is.False);
                Time.timeScale = 1;
                int objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
                Assert.That(special.TryUse(), Is.True);
                Assert.That(special.Charges, Is.EqualTo(1));
                Assert.That(combat.TryFire(), Is.True, "Primary fire remains available during a special.");
                if (choice == SpecialWeaponType.Laser)
                {
                    Assert.That(special.LaserActive, Is.True);
                    Assert.That(special.TryUse(), Is.False, "A laser cannot be restarted while active.");
                    special.CaptureEnemies();
                    special.Simulate(2f);
                    Assert.That(special.LaserActive, Is.False);
                }
                else Assert.That(special.ActiveShots, Is.EqualTo(choice == SpecialWeaponType.Spread ? 7 : 4));
                Assert.That(special.TryUse(), Is.True);
                Assert.That(special.Charges, Is.Zero);
                Assert.That(special.TryUse(), Is.False);
                Assert.That(Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, Is.EqualTo(objects));
                yield return SceneManager.LoadSceneAsync("Gameplay");
                yield return null;
                special = Object.FindAnyObjectByType<PrototypeSpecialWeapon>();
                Assert.That(special.Selected, Is.EqualTo(choice));
                Assert.That(special.Charges, Is.EqualTo(2));
            }
            SpecialWeaponSelection.Selected = SpecialWeaponType.Laser;
        }

        [UnityTest]
        public IEnumerator MissilesTurnTowardAnOffCenterTarget()
        {
            SpecialWeaponSelection.Selected = SpecialWeaponType.Missiles;
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var special = Object.FindAnyObjectByType<PrototypeSpecialWeapon>();
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            for (int i = 0; i < 100 && flight.ActiveEnemies == 0; i++) flight.Simulate(.02f, Vector2.zero, Vector2.zero);
            int enemy = Enumerable.Range(0, flight.Capacity).First(i => flight.GetEnemy(i).gameObject.activeSelf);
            flight.GetEnemy(enemy).localPosition = flight.Aircraft.localPosition + new Vector3(2.5f, 0f, 6f);
            Assert.That(special.TryUse(), Is.True);
            for (int i = 0; i < 90 && flight.GetEnemyHealth(enemy) == 2; i++)
            {
                special.CaptureEnemies();
                special.Simulate(.02f);
            }
            Assert.That(flight.GetEnemyHealth(enemy), Is.LessThan(2));
            SpecialWeaponSelection.Selected = SpecialWeaponType.Laser;
        }

        [UnityTest]
        public IEnumerator AllSpecialTypesDamageEnemies()
        {
            foreach (SpecialWeaponType choice in new[] { SpecialWeaponType.Laser, SpecialWeaponType.Spread, SpecialWeaponType.Missiles })
            {
                SpecialWeaponSelection.Selected = choice;
                yield return SceneManager.LoadSceneAsync("Gameplay");
                yield return null;
                var special = Object.FindAnyObjectByType<PrototypeSpecialWeapon>();
                var flight = Object.FindAnyObjectByType<PrototypeFlight>();
                for (int i = 0; i < 100 && flight.ActiveEnemies == 0; i++) flight.Simulate(.02f, Vector2.zero, Vector2.zero);
                int index = Enumerable.Range(0, flight.Capacity).First(i => flight.GetEnemy(i).gameObject.activeSelf);
                flight.GetEnemy(index).localPosition = flight.Aircraft.localPosition + new Vector3(choice == SpecialWeaponType.Missiles ? 1f : 0f, 0f, choice == SpecialWeaponType.Laser ? 5f : 3f);
                Assert.That(special.TryUse(), Is.True);
                for (int i = 0; i < 90 && flight.GetEnemyHealth(index) == 2; i++)
                {
                    special.CaptureEnemies();
                    special.Simulate(.02f);
                }
                Assert.That(flight.GetEnemyHealth(index), Is.LessThan(2), choice.ToString());
            }
            SpecialWeaponSelection.Selected = SpecialWeaponType.Laser;
        }
    }
}

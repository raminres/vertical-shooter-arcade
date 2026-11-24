using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using VerticalShooter.Gameplay;

namespace VerticalShooter.Tests
{
    public class PrototypeFlightTests
    {
        [UnityTest]
        public IEnumerator AircraftStaysFullyVisibleAtBothScreenEdges()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            foreach (float aspect in new[] { 1179f / 2556f, 1488f / 2266f })
            {
                flight.FlightCamera.aspect = aspect;
                foreach (float direction in new[] { -1f, 1f })
                {
                    for (int i = 0; i < 120; i++) flight.Simulate(.02f, new Vector2(direction, direction), Vector2.zero);
                    var renderers = flight.Aircraft.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        var b = renderer.bounds;
                        foreach (float x in new[] { b.min.x, b.max.x })
                        foreach (float z in new[] { b.min.z, b.max.z })
                        {
                            var view = flight.FlightCamera.WorldToViewportPoint(new Vector3(x, b.center.y, z));
                            Assert.That(view.x, Is.InRange(0f, 1f), $"x at aspect {aspect}");
                            Assert.That(view.y, Is.InRange(0f, 1f), $"y at aspect {aspect}");
                        }
                    }
                }
            }
            flight.FlightCamera.ResetAspect();
        }

        [UnityTest]
        public IEnumerator EnemiesUseThePreallocatedPoolAndCycleGroups()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            var all = flight.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("LightFighter")).ToArray();
            Assert.That(all.Length, Is.EqualTo(3));
            int appearances = 0;
            var previous = new bool[all.Length];
            for (int step = 0; step < 1000; step++)
            {
                flight.Simulate(.1f, Vector2.zero, Vector2.zero);
                Assert.That(flight.ActiveEnemies, Is.InRange(0, flight.Capacity));
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i].gameObject.activeSelf && !previous[i]) appearances++;
                    previous[i] = all[i].gameObject.activeSelf;
                }
            }
            Assert.That(appearances, Is.GreaterThan(8), "Slots should be reused over multiple groups.");
            Assert.That(flight.GetComponentsInChildren<Transform>(true).Count(t => t.name.StartsWith("LightFighter")), Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator KeyboardMovesAircraftAndTouchDragMovesItWithoutJumping()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var touchscreen = InputSystem.AddDevice<Touchscreen>();
            try
            {
                var startX = flight.Aircraft.localPosition.x;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
                yield return null;
                yield return null;
                Assert.That(flight.Aircraft.localPosition.x, Is.GreaterThan(startX));
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null;
                float width = UnityEngine.Device.Screen.width;
                float height = UnityEngine.Device.Screen.height;
                var initial = new Vector2(width * .5f, height * .45f);
                var before = flight.Aircraft.localPosition.x;
                InputSystem.QueueStateEvent(touchscreen, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Began, position = initial });
                yield return null;
                Assert.That(flight.Aircraft.localPosition.x, Is.EqualTo(before).Within(.01f));
                InputSystem.QueueStateEvent(touchscreen, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Moved, position = initial + new Vector2(width * .12f, 0f) });
                yield return null;
                Assert.That(flight.Aircraft.localPosition.x, Is.GreaterThan(before));
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard);
                InputSystem.RemoveDevice(touchscreen);
            }
        }

        [UnityTest]
        public IEnumerator PauseStopsScrollingAndEnemyMovement()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var flight = Object.FindAnyObjectByType<PrototypeFlight>();
            var originalScale = Time.timeScale;
            try
            {
                Time.timeScale = 0f;
                var start = flight.transform.position;
                yield return new WaitForSecondsRealtime(.15f);
                Assert.That(flight.transform.position, Is.EqualTo(start));
            }
            finally { Time.timeScale = originalScale; }
        }
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace VerticalShooter.Tests
{
    public class PrototypeUiTests
    {
        [UnityTest]
        public IEnumerator MainMenuCanOpenPanelsSwitchLanguageAndStartGame()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return LocalizationSettings.InitializationOperation;
            yield return null;
            var root = Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
            Assert.That(root.Q<Button>("continue").enabledSelf, Is.False);

            Click(root.Q<Button>("options"));
            Assert.That(root.Q<VisualElement>("options-panel").ClassListContains("hidden"), Is.False);
            Click(root.Q<Button>("turkish"));
            for (int i = 0; i < 60 && root.Q<Button>("new-game").text != "Yeni Oyun"; i++)
                yield return new WaitForSecondsRealtime(0.02f);
            Assert.That(root.Q<Button>("new-game").text, Is.EqualTo("Yeni Oyun"));
            Click(root.Q<Button>("english"));
            for (int i = 0; i < 60 && root.Q<Button>("new-game").text != "New Game"; i++)
                yield return new WaitForSecondsRealtime(0.02f);
            Assert.That(root.Q<Button>("new-game").text, Is.EqualTo("New Game"));
            Click(root.Q<Button>("options-back"));
            Assert.That(root.Q<VisualElement>("options-panel").ClassListContains("hidden"), Is.True);

            Click(root.Q<Button>("credits"));
            Assert.That(root.Q<VisualElement>("credits-panel").ClassListContains("hidden"), Is.False);
            Click(root.Q<Button>("credits-back"));
            Click(root.Q<Button>("new-game"));
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Gameplay"));
        }

        [UnityTest]
        public IEnumerator PauseOptionsResumeAndReturnToMainMenu()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
            var root = Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
            Assert.That(root.Q<Button>("special").enabledSelf, Is.False);
            Click(root.Q<Button>("pause"));
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(root.Q<VisualElement>("pause-panel").ClassListContains("hidden"), Is.False);
            Click(root.Q<Button>("options"));
            Assert.That(root.Q<VisualElement>("options-panel").ClassListContains("hidden"), Is.False);
            Click(root.Q<Button>("options-back"));
            Click(root.Q<Button>("resume"));
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Click(root.Q<Button>("pause"));
            Click(root.Q<Button>("main-menu"));
            yield return null;
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MainMenu"));
        }

        [UnityTest]
        public IEnumerator ButtonAnimatesForMouseHoverAndPressWhilePaused()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;
            var root = Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
            var button = root.Q<Button>("options");
            try
            {
                using (var enter = PointerEnterEvent.GetPooled(new Event { type = EventType.MouseMove, mousePosition = button.worldBound.center }))
                {
                    enter.target = button;
                    button.SendEvent(enter);
                }
                yield return new WaitForSecondsRealtime(0.3f);
                Assert.That(button.resolvedStyle.scale.value.x, Is.GreaterThan(1.01f));

                Time.timeScale = 0f;
                var mouseDown = new Event { type = EventType.MouseDown, button = 0, mousePosition = button.worldBound.center };
                using (var down = PointerDownEvent.GetPooled(mouseDown))
                {
                    down.target = button;
                    button.SendEvent(down);
                }
                yield return new WaitForSecondsRealtime(0.3f);
                Assert.That(button.resolvedStyle.scale.value.x, Is.LessThan(0.97f));
                Assert.That(button.resolvedStyle.backgroundColor.r, Is.LessThan(0.15f));
            }
            finally
            {
                Time.timeScale = 1f;
            }
        }

        [UnityTest]
        public IEnumerator EveryEnabledMenuButtonRespondsToMouseHoverButNotTouchHover()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;
            var root = Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
            foreach (var name in new[] { "new-game", "options", "credits", "continue" })
            {
                var button = root.Q<Button>(name);
                var mouseMove = new Event { type = EventType.MouseMove, mousePosition = button.worldBound.center };
                using (var enter = PointerEnterEvent.GetPooled(mouseMove))
                {
                    enter.target = button;
                    button.SendEvent(enter);
                }
                yield return new WaitForSecondsRealtime(0.25f);
                Assert.That(button.resolvedStyle.scale.value.x, button.enabledSelf ? Is.GreaterThan(1.01f) : Is.EqualTo(1f).Within(0.01f), name);
                using (var leave = PointerLeaveEvent.GetPooled(mouseMove))
                {
                    leave.target = button;
                    button.SendEvent(leave);
                }
                yield return new WaitForSecondsRealtime(0.25f);
                Assert.That(button.resolvedStyle.scale.value.x, Is.EqualTo(1f).Within(0.01f), name);
            }

            var options = root.Q<Button>("options");
            var touch = new Touch { fingerId = 1, position = options.worldBound.center, phase = TouchPhase.Began };
            using (var enter = PointerEnterEvent.GetPooled(touch, EventModifiers.None))
            {
                enter.target = options;
                options.SendEvent(enter);
            }
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.That(options.ClassListContains("mouse-hover"), Is.False);
            Assert.That(options.resolvedStyle.scale.value.x, Is.EqualTo(1f).Within(0.01f));
        }

        private static void Click(Button button)
        {
            using (var evt = NavigationSubmitEvent.GetPooled())
            {
                evt.target = button;
                button.SendEvent(evt);
            }
        }
    }
}

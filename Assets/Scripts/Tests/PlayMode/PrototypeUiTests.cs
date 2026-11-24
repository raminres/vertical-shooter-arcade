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
                yield return null;
            Assert.That(root.Q<Button>("new-game").text, Is.EqualTo("Yeni Oyun"));
            Click(root.Q<Button>("english"));
            for (int i = 0; i < 60 && root.Q<Button>("new-game").text != "New Game"; i++)
                yield return null;
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

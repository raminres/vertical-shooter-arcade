using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace VerticalShooter.UI
{
    public sealed class GameplayUiController : ScreenController
    {
        private bool paused;
        private float previousTimeScale = 1f;

        protected override void OnEnable()
        {
            base.OnEnable();
            Bind("health-label", "health");
            Bind("stars-label", "stars");
            Bind("special", "special_charges");
            Bind("pause-title", "paused");
            Bind("resume", "resume");
            Bind("options", "options");
            Bind("main-menu", "main_menu");
            BindOptions();
            root.Q<Button>("special").SetEnabled(false);
            root.Q<Button>("pause").clicked += Pause;
            root.Q<Button>("resume").clicked += Resume;
            root.Q<Button>("options").clicked += OpenOptions;
            root.Q<Button>("options-back").clicked += CloseOptions;
            root.Q<Button>("main-menu").clicked += ReturnToMenu;
        }

        protected override void OnDisable()
        {
            root.Q<Button>("pause").clicked -= Pause;
            root.Q<Button>("resume").clicked -= Resume;
            root.Q<Button>("options").clicked -= OpenOptions;
            root.Q<Button>("options-back").clicked -= CloseOptions;
            root.Q<Button>("main-menu").clicked -= ReturnToMenu;
            RestoreTime();
            base.OnDisable();
        }

        public void Pause()
        {
            if (paused) return;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            paused = true;
            Show("pause-panel", true);
        }

        public void Resume()
        {
            Show("options-panel", false);
            Show("pause-panel", false);
            RestoreTime();
        }

        private void RestoreTime()
        {
            if (!paused) return;
            Time.timeScale = previousTimeScale;
            paused = false;
        }

        private void OpenOptions() => Show("options-panel", true);
        private void CloseOptions() => Show("options-panel", false);
        private void ReturnToMenu()
        {
            RestoreTime();
            SceneManager.LoadScene("MainMenu");
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Localization;
using VerticalShooter.Gameplay;

namespace VerticalShooter.UI
{
    public sealed class GameplayUiController : ScreenController
    {
        [SerializeField] private PrototypeCombat combat;
        [SerializeField] private PrototypeSpecialWeapon special;
        private LocalizedString specialLabel;
        private string specialText = "SPECIAL";
        private bool paused;
        private float previousTimeScale = 1f;

        protected override void OnEnable()
        {
            base.OnEnable();
            Bind("health-label", "health");
            Bind("stars-label", "stars");

            Bind("pause-title", "paused");
            Bind("failed-title", "level_failed");
            Bind("retry", "retry");
            Bind("failed-main-menu", "main_menu");
            Bind("resume", "resume");
            Bind("options", "options");
            Bind("main-menu", "main_menu");
            BindOptions();
            specialLabel = new LocalizedString("UI", "special");
            specialLabel.StringChanged += OnSpecialLabelChanged;
            special.ChargesChanged += RefreshSpecial;
            root.Q<Button>("special").clicked += UseSpecial;
            root.Q<Button>("pause").clicked += Pause;
            root.Q<Button>("resume").clicked += Resume;
            root.Q<Button>("options").clicked += OpenOptions;
            root.Q<Button>("options-back").clicked += CloseOptions;
            root.Q<Button>("main-menu").clicked += ReturnToMenu;
            root.Q<Button>("failed-main-menu").clicked += ReturnToMenu;
            root.Q<Button>("retry").clicked += Retry;
            combat.PlayerHealthChanged += UpdateHealth;
        }

        private void Start()
        {
            UpdateHealth(combat.PlayerHealth, combat.MaxPlayerHealth);
            RefreshSpecial();
        }

        private void OnSpecialLabelChanged(string value) { specialText = value; RefreshSpecial(); }
        private void UseSpecial() { if (!paused) special.TryUse(); }
        private void RefreshSpecial()
        {
            var button = root.Q<Button>("special");
            button.text = $"{specialText}  ·  {special.Charges}";
            button.SetEnabled(special.Charges > 0 && !special.LaserActive && !combat.IsDefeated);
        }

        private void UpdateHealth(int health, int maximum)
        {
            root.Q<Label>("health-value").text = $"{health} / {maximum}";
            if (health == 0)
            {
                root.Q<Button>("pause").SetEnabled(false);
                Show("failed-panel", true);
                RefreshSpecial();
            }
        }

        private void Retry() => SceneManager.LoadScene("Gameplay");

        protected override void OnDisable()
        {
            specialLabel.StringChanged -= OnSpecialLabelChanged;
            special.ChargesChanged -= RefreshSpecial;
            root.Q<Button>("special").clicked -= UseSpecial;
            root.Q<Button>("pause").clicked -= Pause;
            root.Q<Button>("resume").clicked -= Resume;
            root.Q<Button>("options").clicked -= OpenOptions;
            root.Q<Button>("options-back").clicked -= CloseOptions;
            root.Q<Button>("main-menu").clicked -= ReturnToMenu;
            root.Q<Button>("failed-main-menu").clicked -= ReturnToMenu;
            root.Q<Button>("retry").clicked -= Retry;
            combat.PlayerHealthChanged -= UpdateHealth;
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

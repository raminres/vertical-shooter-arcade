using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace VerticalShooter.UI
{
    public sealed class MainMenuController : ScreenController
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            Bind("new-game", "new_game");
            Bind("continue", "continue");
            Bind("options", "options");
            Bind("credits", "credits");
            Bind("credits-title", "credits");
            Bind("credits-body", "credits_body");
            Bind("credits-back", "back");
            BindOptions();

            root.Q<Button>("continue").SetEnabled(false);
            root.Q<Button>("new-game").clicked += StartGame;
            root.Q<Button>("options").clicked += OpenOptions;
            root.Q<Button>("credits").clicked += OpenCredits;
            root.Q<Button>("options-back").clicked += CloseOptions;
            root.Q<Button>("credits-back").clicked += CloseCredits;
        }

        protected override void OnDisable()
        {
            root.Q<Button>("new-game").clicked -= StartGame;
            root.Q<Button>("options").clicked -= OpenOptions;
            root.Q<Button>("credits").clicked -= OpenCredits;
            root.Q<Button>("options-back").clicked -= CloseOptions;
            root.Q<Button>("credits-back").clicked -= CloseCredits;
            base.OnDisable();
        }

        private void StartGame() => SceneManager.LoadScene("Gameplay");
        private void OpenOptions() => Show("options-panel", true);
        private void CloseOptions() => Show("options-panel", false);
        private void OpenCredits() => Show("credits-panel", true);
        private void CloseCredits() => Show("credits-panel", false);
    }
}

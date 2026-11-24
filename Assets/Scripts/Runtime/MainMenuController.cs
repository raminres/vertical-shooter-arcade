using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VerticalShooter.Gameplay;

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
            Bind("weapon-title", "choose_special");
            Bind("choose-laser", "focused_laser");
            Bind("choose-spread", "spread_barrage");
            Bind("choose-missiles", "homing_missiles");
            Bind("weapon-back", "back");
            BindOptions();

            root.Q<Button>("continue").SetEnabled(false);
            root.Q<Button>("new-game").clicked += OpenWeaponSelection;
            root.Q<Button>("choose-laser").clicked += StartLaser;
            root.Q<Button>("choose-spread").clicked += StartSpread;
            root.Q<Button>("choose-missiles").clicked += StartMissiles;
            root.Q<Button>("weapon-back").clicked += CloseWeaponSelection;
            root.Q<Button>("options").clicked += OpenOptions;
            root.Q<Button>("credits").clicked += OpenCredits;
            root.Q<Button>("options-back").clicked += CloseOptions;
            root.Q<Button>("credits-back").clicked += CloseCredits;
        }

        protected override void OnDisable()
        {
            root.Q<Button>("new-game").clicked -= OpenWeaponSelection;
            root.Q<Button>("choose-laser").clicked -= StartLaser;
            root.Q<Button>("choose-spread").clicked -= StartSpread;
            root.Q<Button>("choose-missiles").clicked -= StartMissiles;
            root.Q<Button>("weapon-back").clicked -= CloseWeaponSelection;
            root.Q<Button>("options").clicked -= OpenOptions;
            root.Q<Button>("credits").clicked -= OpenCredits;
            root.Q<Button>("options-back").clicked -= CloseOptions;
            root.Q<Button>("credits-back").clicked -= CloseCredits;
            base.OnDisable();
        }

        private void OpenWeaponSelection() => Show("weapon-panel", true);
        private void CloseWeaponSelection() => Show("weapon-panel", false);
        private void StartLaser() => StartGame(SpecialWeaponType.Laser);
        private void StartSpread() => StartGame(SpecialWeaponType.Spread);
        private void StartMissiles() => StartGame(SpecialWeaponType.Missiles);
        private void StartGame(SpecialWeaponType choice)
        {
            SpecialWeaponSelection.Selected = choice;
            SceneManager.LoadScene("Gameplay");
        }
        private void OpenOptions() => Show("options-panel", true);
        private void CloseOptions() => Show("options-panel", false);
        private void OpenCredits() => Show("credits-panel", true);
        private void CloseCredits() => Show("credits-panel", false);
    }
}

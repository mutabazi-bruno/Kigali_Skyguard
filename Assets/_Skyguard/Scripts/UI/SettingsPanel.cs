using System;
using Skyguard.Data;
using Skyguard.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace Skyguard.UI
{
    // each button cycles one setting and shows its current value as the label
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] Button difficultyButton;
        [SerializeField] Button languageButton;
        [SerializeField] Button handButton;
        [SerializeField] Button positionButton;
        [SerializeField] Button snapTurnButton;
        [SerializeField] Button shieldButton;
        [SerializeField] Button volumeButton;
        [SerializeField] Button fpsButton;
        [SerializeField] Button tutorialButton;
        [SerializeField] Button backButton;

        public event Action Back;

        static GameManager Game => GameManager.Instance;
        static PlayerSettings S => Game.Settings;

        void Awake()
        {
            UiButtons.Hook(difficultyButton, () => Change(() => S.difficulty = S.difficulty == Difficulty.Easy ? Difficulty.Normal : Difficulty.Easy));
            UiButtons.Hook(languageButton, () => Change(() => S.language = (SubtitleLanguage)(((int)S.language + 1) % 3)));
            UiButtons.Hook(handButton, () => Change(() => S.leftHanded = !S.leftHanded));
            UiButtons.Hook(positionButton, () => Change(() => S.seated = !S.seated));
            UiButtons.Hook(snapTurnButton, () => Change(() => S.snapTurn = !S.snapTurn));
            UiButtons.Hook(shieldButton, () => Change(() => S.frontShield = !S.frontShield));
            UiButtons.Hook(volumeButton, () => Change(() => S.volume = S.volume >= 0.99f ? 0.2f : Mathf.Min(1f, S.volume + 0.2f)));
            UiButtons.Hook(fpsButton, () => Change(() => S.showFps = !S.showFps));
            UiButtons.Hook(tutorialButton, () =>
            {
                Game.ReplayTutorialNextRun();
                UiButtons.SetLabel(tutorialButton, "Tutorial will play next run");
            });
            UiButtons.Hook(backButton, () => Back?.Invoke());
        }

        void OnEnable() => Refresh();

        void Change(Action edit)
        {
            edit();
            Game.ApplySettings();
            Refresh();
        }

        void Refresh()
        {
            UiButtons.SetLabel(difficultyButton, S.difficulty == Difficulty.Easy ? "Difficulty: Easy (slower drones)" : "Difficulty: Normal");
            UiButtons.SetLabel(languageButton, "Subtitles: " + S.language switch
            {
                SubtitleLanguage.French => "Français",
                SubtitleLanguage.Kinyarwanda => "Kinyarwanda",
                _ => "English"
            });
            UiButtons.SetLabel(handButton, S.leftHanded ? "Blaster hand: Left" : "Blaster hand: Right");
            UiButtons.SetLabel(positionButton, S.seated ? "Playing: Seated" : "Playing: Standing");
            UiButtons.SetLabel(snapTurnButton, S.snapTurn ? "Snap turn: On" : "Snap turn: Off");
            string key = S.leftHanded ? "B" : "Y";
            UiButtons.SetLabel(shieldButton, (S.frontShield ? "Hand shield: On" : "Hand shield: Off") + $"  ({key} button toggles)");
            UiButtons.SetLabel(volumeButton, $"Volume: {Mathf.RoundToInt(S.volume * 100)}%");
            UiButtons.SetLabel(fpsButton, S.showFps ? "FPS on wrist: On" : "FPS on wrist: Off");
            UiButtons.SetLabel(tutorialButton, "Replay tutorial");
        }
    }
}

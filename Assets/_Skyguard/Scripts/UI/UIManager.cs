using Skyguard.Core;
using Skyguard.Flow;
using Skyguard.Platform;
using Skyguard.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skyguard.UI
{
    // swaps the world-space menu panels as the game state changes and forwards clicks to the GameManager
    public class UIManager : MonoBehaviour
    {
        [SerializeField] Transform head;
        [SerializeField] Transform menuRoot;
        [SerializeField] float distance = 1.5f;
        [SerializeField] AudioManager audioManager;

        [Header("Panels")]
        [SerializeField] ComfortPanel comfort;
        [SerializeField] GameObject mainPanel;
        [SerializeField] SettingsPanel settings;
        [SerializeField] LeaderboardPanel leaderboard;
        [SerializeField] GameObject pausePanel;
        [SerializeField] ResultsPanel results;

        [Header("Main menu")]
        [SerializeField] Button campaignButton;
        [SerializeField] Button endlessButton;
        [SerializeField] Button leaderboardButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button introButton;
        [SerializeField] Button quitButton;

        [Header("Pause")]
        [SerializeField] Button resumeButton;
        [SerializeField] Button restartButton;
        [SerializeField] Button quitToMenuButton;

        GameObject[] panels;
        float placeAt = -1f;

        static GameManager Game => GameManager.Instance;

        void Awake()
        {
            panels = new[] { comfort.gameObject, mainPanel, settings.gameObject, leaderboard.gameObject, pausePanel, results.gameObject };
            UiButtons.ClickSound = audioManager.PlayUiClick;

            UiButtons.Hook(campaignButton, () => Game.StartCampaign());
            UiButtons.Hook(endlessButton, () => Game.StartEndless());
            UiButtons.Hook(leaderboardButton, () => Show(leaderboard.gameObject));
            UiButtons.Hook(settingsButton, () => Show(settings.gameObject));
            UiButtons.Hook(introButton, SceneFlow.PlayIntro);
            UiButtons.Hook(quitButton, () => Game.QuitGame());

            UiButtons.Hook(resumeButton, () => Game.Resume());
            UiButtons.Hook(restartButton, () => Game.RestartWave());
            UiButtons.Hook(quitToMenuButton, () => Game.QuitToMenu());

            comfort.Done += () => Show(mainPanel);
            settings.Back += () => Show(mainPanel);
            leaderboard.Back += () => Show(mainPanel);
        }

        void OnEnable()
        {
            EventBus.Subscribe<GameStateChanged>(OnStateChanged);
            EventBus.Subscribe<RunEnded>(OnRunEnded);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<GameStateChanged>(OnStateChanged);
            EventBus.Unsubscribe<RunEnded>(OnRunEnded);
        }

        void OnStateChanged(GameStateChanged e)
        {
            switch (e.State)
            {
                case GameStateId.MainMenu:
                    Show(Game.NeedsComfortSetup ? comfort.gameObject : mainPanel);
                    break;
                case GameStateId.Paused:
                    Show(pausePanel);
                    break;
                case GameStateId.Results:
                    // the results panel was already opened by RunEnded, which arrives first
                    break;
                default:
                    menuRoot.gameObject.SetActive(false);
                    break;
            }
        }

        void OnRunEnded(RunEnded e)
        {
            results.Show(e.Result);
            Show(results.gameObject);
        }

        void Show(GameObject panel)
        {
            foreach (var p in panels)
                p.SetActive(p == panel);

            if (panel == mainPanel)
                RefreshMain();

            // the headset pose isn't tracked yet on the first frames after launch, so place it a moment later
            if (!menuRoot.gameObject.activeSelf)
                placeAt = Time.unscaledTime + 0.25f;
            menuRoot.gameObject.SetActive(true);
        }

        void LateUpdate()
        {
            if (placeAt < 0f || Time.unscaledTime < placeAt)
                return;
            placeAt = -1f;
            PlaceInFront();
        }

        void RefreshMain()
        {
            bool unlocked = Game.EndlessUnlocked;
            endlessButton.interactable = unlocked;
            UiButtons.SetLabel(endlessButton, unlocked ? "Endless" : "Endless (finish the night first)");
        }

        void PlaceInFront()
        {
            var forward = head.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.01f)
                forward = head.up;
            forward.Normalize();

            menuRoot.position = head.position + forward * distance + Vector3.down * 0.1f;
            menuRoot.rotation = Quaternion.LookRotation(forward);
        }
    }
}

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
        [Tooltip("How far you can turn away before the menu slides back in front of you.")]
        [SerializeField] float followAngle = 35f;
        [SerializeField] float followSpeed = 5f;
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
        [SerializeField] TextMeshProUGUI bestScore;

        [Header("Pause")]
        [SerializeField] Button resumeButton;
        [SerializeField] Button restartButton;
        [SerializeField] Button quitToMenuButton;

        GameObject[] panels;
        float placeAt = -1f;
        bool gliding;
        Vector3 goalPosition;
        Quaternion goalRotation;

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
            if (!menuRoot.gameObject.activeSelf)
                return;

            if (placeAt >= 0f)
            {
                if (Time.unscaledTime < placeAt)
                    return;
                placeAt = -1f;
                gliding = false;
                FindSpotInFront(out goalPosition, out goalRotation);
                menuRoot.SetPositionAndRotation(goalPosition, goalRotation);
                return;
            }

            if (!gliding && LeftBehind())
            {
                FindSpotInFront(out goalPosition, out goalRotation);
                gliding = true;
            }
            if (!gliding)
                return;

            float k = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
            menuRoot.SetPositionAndRotation(Vector3.Lerp(menuRoot.position, goalPosition, k),
                                            Quaternion.Slerp(menuRoot.rotation, goalRotation, k));
            if ((menuRoot.position - goalPosition).sqrMagnitude < 0.0001f)
                gliding = false;
        }

        // small head movements while aiming at a button shouldn't drag the menu around,
        // only turning well away, walking off or sitting down does
        bool LeftBehind()
        {
            var toMenu = menuRoot.position - head.position;
            float height = toMenu.y;
            toMenu.y = 0f;
            var forward = head.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.01f)
                return false;

            return Vector3.Angle(forward, toMenu) > followAngle
                || Mathf.Abs(toMenu.magnitude - distance) > 0.5f
                || Mathf.Abs(height + 0.1f) > 0.35f;
        }

        void RefreshMain()
        {
            bool unlocked = Game.EndlessUnlocked;
            endlessButton.interactable = unlocked;
            UiButtons.SetLabel(endlessButton, unlocked ? "Endless" : "Endless (locked)");

            var top = Game.Leaderboard.TopScores;
            bestScore.text = top.Count > 0
                ? $"Best score   <color=#F5B83D>{top[0].initials}  {top[0].score:N0}</color>"
                : "No scores yet. Be the first on the board.";
        }

        void FindSpotInFront(out Vector3 position, out Quaternion rotation)
        {
            var forward = head.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.01f)
                forward = head.up;
            forward.Normalize();

            position = head.position + forward * distance + Vector3.down * 0.1f;
            rotation = Quaternion.LookRotation(forward);
        }
    }
}

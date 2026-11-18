using System.Collections;
using Skyguard.Core;
using Skyguard.Data;
using Skyguard.Gameplay;
using Skyguard.Platform;
using Skyguard.Upgrades;
using Skyguard.Waves;
using Skyguard.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skyguard.Flow
{
    // the one singleton. owns the state machine and is the only thing the menus call into
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] RooftopRig rooftops;
        [SerializeField] WaveManager waves;
        [SerializeField] Blaster blaster;
        [SerializeField] BlasterHolster holster;
        [SerializeField] PlayerShield playerShield;
        [SerializeField] ScoreKeeper score;
        [SerializeField] UpgradeSelector upgrades;
        [SerializeField] TutorialController tutorial;
        [SerializeField] PlayerRig playerRig;
        [SerializeField] FrameTimer frameTimer;

        readonly GameStateMachine machine = new();
        InputAction pauseAction;
        InputAction shieldLeft;
        InputAction shieldRight;
        bool starting;
        int waveStartScore;
        int waveStartTowerHealth;

        public LeaderboardRepository Leaderboard { get; private set; }
        public PlayerSettings Settings => Leaderboard.Settings;
        public GameMode Mode { get; private set; }
        public int Wave { get; private set; }
        public int TotalWaves => Mode == GameMode.Endless ? 0 : waves.CampaignLength;
        public bool EndlessUnlocked => Leaderboard.Data.campaignCleared;
        public bool NeedsComfortSetup => !Leaderboard.Data.comfortSetupDone;
        public GameStateId State => machine.Current.Id;
        public RelayTower Tower => rooftops.Current.Tower;

        internal WaveManager Waves => waves;
        internal UpgradeSelector Upgrades => upgrades;
        internal TutorialController Tutorial => tutorial;
        internal PlayerShield PlayerShield => playerShield;
        internal RooftopRig Rooftops => rooftops;
        internal BlasterHolster Holster => holster;
        internal Blaster Blaster => blaster;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Leaderboard = new LeaderboardRepository(new JsonSaveStorage());
            Leaderboard.Load();

            pauseAction = new InputAction("Pause", binding: "<XRController>{LeftHand}/{MenuButton}");
            pauseAction.AddBinding("<Keyboard>/escape");

            // top face button of the off hand: Y normally, B for left-handed players
            shieldLeft = new InputAction("Shield Left", binding: "<XRController>{LeftHand}/{SecondaryButton}");
            shieldLeft.AddBinding("<Keyboard>/t");
            shieldRight = new InputAction("Shield Right", binding: "<XRController>{RightHand}/{SecondaryButton}");
        }

        void OnEnable()
        {
            pauseAction.performed += OnPausePressed;
            pauseAction.Enable();
            shieldLeft.performed += OnShieldLeft;
            shieldRight.performed += OnShieldRight;
            shieldLeft.Enable();
            shieldRight.Enable();
        }

        void OnDisable()
        {
            pauseAction.performed -= OnPausePressed;
            pauseAction.Disable();
            shieldLeft.performed -= OnShieldLeft;
            shieldRight.performed -= OnShieldRight;
            shieldLeft.Disable();
            shieldRight.Disable();
        }

        void Start()
        {
            ApplySettings();
            machine.ChangeState(new MainMenuState(this));
        }

        void Update() => machine.Tick(Time.deltaTime);

        // taking the headset off pauses the run
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                PauseIfPlaying();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
                PauseIfPlaying();
        }

        // ---------------------------------------------------------------- called by the menus

        public void StartCampaign() => TryBeginRun(GameMode.Campaign);

        public void StartEndless()
        {
            if (EndlessUnlocked)
                TryBeginRun(GameMode.Endless);
        }

        public void Pause()
        {
            bool playing = State is GameStateId.Wave or GameStateId.Upgrade or GameStateId.Tutorial;
            if (playing && !rooftops.IsMoving)
                machine.PushOverlay(new PausedState());
        }

        public void Resume() => machine.PopOverlay();

        public void RestartWave()
        {
            if (!machine.HasOverlay)
                return;
            score.Restore(waveStartScore);
            Tower.SetHealth(waveStartTowerHealth);
            machine.ChangeState(new WaveState(this));
        }

        public void QuitToMenu() => machine.ChangeState(new MainMenuState(this));

        public void QuitGame()
        {
            Leaderboard.Save();
            Application.Quit();
        }

        public void ApplySettings()
        {
            Leaderboard.Save();
            playerRig.Apply(Settings);
            waves.DifficultySpeed = Settings.difficulty == Difficulty.Easy ? 0.75f : 1f;
            AudioListener.volume = Settings.volume;
            EventBus.Publish(new SettingsChanged { Settings = Settings });
        }

        public void CompleteComfortSetup()
        {
            Leaderboard.Data.comfortSetupDone = true;
            ApplySettings();
        }

        public void RenameEntry(ScoreEntry entry, string initials) => Leaderboard.Rename(entry, initials);

        public void ReplayTutorialNextRun()
        {
            Leaderboard.Data.tutorialDone = false;
            Leaderboard.Save();
        }

        // ---------------------------------------------------------------- called by the states

        void TryBeginRun(GameMode mode)
        {
            // a double click on the menu shouldn't start two runs
            if (starting || State != GameStateId.MainMenu)
                return;
            StartCoroutine(BeginRun(mode));
        }

        IEnumerator BeginRun(GameMode mode)
        {
            starting = true;
            Mode = mode;
            Wave = 1;
            score.ResetRun();
            frameTimer.ResetRun();
            waves.LaserSpeedMultiplier = 1f;
            blaster.Well.SnapRadius = 0.07f;
            for (int i = 0; i < rooftops.Count; i++)
                rooftops.Get(i).Tower.ResetHealth();

            int roof = RooftopForWave(Wave);
            if (roof != rooftops.CurrentIndex)
                yield return rooftops.MoveTo(roof);
            UseRooftop(roof);
            blaster.ResetForRun();

            bool tutorialFirst = mode == GameMode.Campaign && !Leaderboard.Data.tutorialDone;
            starting = false;
            machine.ChangeState(tutorialFirst ? new TutorialState(this) : new WaveState(this));
        }

        internal void OnTutorialFinished()
        {
            Leaderboard.Data.tutorialDone = true;
            Leaderboard.Save();
            blaster.LoadFresh();
            machine.ChangeState(new WaveState(this));
        }

        internal void SnapshotWaveStart()
        {
            waveStartScore = score.Score;
            waveStartTowerHealth = Tower.Health;
        }

        internal void OnWaveCleared()
        {
            bool towerBonus = Tower.Health * 2 >= Tower.MaxHealth;
            EventBus.Publish(new WaveCleared { Wave = Wave, TowerBonus = towerBonus });

            if (Mode == GameMode.Campaign && Wave >= waves.CampaignLength)
                EndRun(true);
            else
                machine.ChangeState(new UpgradeState(this));
        }

        internal UpgradeContext UpgradeContext => new()
        {
            Blaster = blaster,
            Tower = Tower,
            Score = score,
            Waves = waves
        };

        internal void OnUpgradePicked() => StartCoroutine(NextWave());

        IEnumerator NextWave()
        {
            Wave++;
            int roof = RooftopForWave(Wave);
            if (roof != rooftops.CurrentIndex)
            {
                EventBus.Publish(new IshemaSpoke { Key = roof == 1 ? "move_t2" : "move_t3" });
                yield return rooftops.MoveTo(roof);
                UseRooftop(roof);
            }
            machine.ChangeState(new WaveState(this));
        }

        internal void EndRun(bool won) => machine.ChangeState(new ResultsState(this, won));

        internal RunResult FinishRun(bool won)
        {
            if (won && Mode == GameMode.Campaign)
                Leaderboard.Data.campaignCleared = true;

            int wavesCleared = won ? Wave : Wave - 1;
            var entry = new ScoreEntry
            {
                initials = Settings.lastInitials,
                score = score.Score,
                waves = wavesCleared,
                accuracy = score.Accuracy,
                mode = Mode,
                difficulty = Settings.difficulty,
                won = won,
                date = System.DateTime.Now.ToString("yyyy-MM-dd")
            };

            int rank = Leaderboard.Submit(entry);
            Leaderboard.Save();
            return new RunResult
            {
                Won = won,
                Mode = Mode,
                Score = score.Score,
                Waves = wavesCleared,
                Accuracy = score.Accuracy,
                Rank = rank,
                AverageFps = frameTimer.AverageFps,
                Entry = rank > 0 ? entry : null
            };
        }

        internal void ResetToMenu()
        {
            waves.Clear();
            upgrades.Cancel();
            tutorial.Stop();
            UseRooftop(0);
            rooftops.PlaceAt(0);
            holster.ReturnNow();
        }

        // waves 1-4 on tower 1, 5-7 on the hill, 8-10 on the main relay. endless lives on the hill
        int RooftopForWave(int wave)
        {
            if (Mode == GameMode.Endless)
                return 1;
            return wave <= 4 ? 0 : wave <= 7 ? 1 : 2;
        }

        void UseRooftop(int index)
        {
            var roof = rooftops.Get(index);
            waves.SetBattlefield(roof.Tower, roof.SpawnArea);
        }

        void PauseIfPlaying()
        {
            if (machine.Current != null)
                Pause();
        }

        void OnShieldLeft(InputAction.CallbackContext _) => ToggleShield(true);
        void OnShieldRight(InputAction.CallbackContext _) => ToggleShield(false);

        // only the hand without the blaster carries the shield, the gun hand's button does nothing
        void ToggleShield(bool leftHand)
        {
            if (leftHand == Settings.leftHanded || Time.timeScale == 0f)
                return;
            Settings.frontShield = !Settings.frontShield;
            ApplySettings();
        }

        void OnPausePressed(InputAction.CallbackContext _)
        {
            if (State == GameStateId.Paused)
                Resume();
            else
                Pause();
        }
    }
}

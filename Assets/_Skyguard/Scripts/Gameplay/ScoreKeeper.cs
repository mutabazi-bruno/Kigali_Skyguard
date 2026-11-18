using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Gameplay
{
    public class ScoreKeeper : MonoBehaviour
    {
        public const int MaxMultiplier = 4;

        [SerializeField] int hitsPerStep = 5;
        [SerializeField] int towerBonus = 500;
        [SerializeField] int blockBonus = 50;

        bool counting;
        bool overdriveArmed;
        bool overdriveActive;
        int streak;

        public int Score { get; private set; }
        public int Multiplier { get; private set; } = 1;
        public int ShotsFired { get; private set; }
        public int Hits { get; private set; }
        public float Accuracy => ShotsFired == 0 ? 0f : (float)Hits / ShotsFired;
        public bool OverdriveArmed => overdriveArmed;

        void OnEnable()
        {
            EventBus.Subscribe<GameStateChanged>(OnStateChanged);
            EventBus.Subscribe<ShotFired>(OnShotFired);
            EventBus.Subscribe<ShotResolved>(OnShotResolved);
            EventBus.Subscribe<DroneDestroyed>(OnDroneDestroyed);
            EventBus.Subscribe<WaveStarted>(OnWaveStarted);
            EventBus.Subscribe<WaveCleared>(OnWaveCleared);
            EventBus.Subscribe<LaserBlocked>(OnLaserBlocked);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<GameStateChanged>(OnStateChanged);
            EventBus.Unsubscribe<ShotFired>(OnShotFired);
            EventBus.Unsubscribe<ShotResolved>(OnShotResolved);
            EventBus.Unsubscribe<DroneDestroyed>(OnDroneDestroyed);
            EventBus.Unsubscribe<WaveStarted>(OnWaveStarted);
            EventBus.Unsubscribe<WaveCleared>(OnWaveCleared);
            EventBus.Unsubscribe<LaserBlocked>(OnLaserBlocked);
        }

        public void ResetRun()
        {
            Score = 0;
            ShotsFired = 0;
            Hits = 0;
            streak = 0;
            Multiplier = 1;
            overdriveArmed = false;
            overdriveActive = false;
            Announce();
        }

        // used when a wave is restarted from the pause menu
        public void Restore(int score)
        {
            Score = score;
            streak = 0;
            Multiplier = 1;
            Announce();
        }

        public void ArmOverdrive() => overdriveArmed = true;

        // practice shots and upgrade cards shouldn't touch accuracy, so only waves count
        void OnStateChanged(GameStateChanged e)
        {
            if (e.State != GameStateId.Paused)
                counting = e.State == GameStateId.Wave;
        }

        void OnShotFired(ShotFired _)
        {
            if (counting)
                ShotsFired++;
        }

        void OnShotResolved(ShotResolved e)
        {
            if (!counting)
                return;

            if (e.HitEnemy)
            {
                Hits++;
                streak++;
                Multiplier = Mathf.Min(MaxMultiplier, 1 + streak / hitsPerStep);
            }
            else
            {
                streak = 0;
                Multiplier = 1;
            }
            Announce();
        }

        void OnDroneDestroyed(DroneDestroyed e)
        {
            if (!e.ByPlayer)
                return;
            Add(e.Points * Multiplier);
        }

        void OnWaveStarted(WaveStarted _)
        {
            overdriveActive = overdriveArmed;
            overdriveArmed = false;
        }

        void OnWaveCleared(WaveCleared e)
        {
            if (e.TowerBonus)
                Add(towerBonus);
            overdriveActive = false;
        }

        void OnLaserBlocked(LaserBlocked _)
        {
            if (counting)
                Add(blockBonus);
        }

        void Add(int points)
        {
            Score += overdriveActive ? points * 2 : points;
            Announce();
        }

        void Announce() => EventBus.Publish(new ScoreChanged { Score = Score, Multiplier = Multiplier });
    }
}

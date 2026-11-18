using Skyguard.Core;
using Skyguard.Drones;
using UnityEngine;

namespace Skyguard.Presentation
{
    // every sound in the game hangs off an event. gameplay code never touches an AudioSource
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] SoundBank sounds;
        [SerializeField] PooledAudio voicePrefab;
        [SerializeField] int voices = 24;
        [SerializeField] AudioSource musicBase;
        [SerializeField] AudioSource musicDrive;
        [SerializeField] AudioLowPassFilter musicFilter;
        [SerializeField] float musicVolume = 0.45f;

        ObjectPool<PooledAudio> pool;
        float driveTarget;
        float cutoffTarget = 22000f;
        float baseTarget;
        bool towerAlarmPlayed;

        void Awake()
        {
            pool = new ObjectPool<PooledAudio>(voicePrefab, voices, transform);
            musicBase.clip = sounds.musicBase;
            musicDrive.clip = sounds.musicDrive;
            // both layers start together and never stop, so they stay in time with each other
            musicBase.Play();
            musicDrive.Play();
            musicDrive.volume = 0f;
            baseTarget = musicVolume * 0.7f;
        }

        void OnEnable()
        {
            EventBus.Subscribe<ShotFired>(OnShot);
            EventBus.Subscribe<DryFired>(OnDryFire);
            EventBus.Subscribe<MagazineEjected>(OnEjected);
            EventBus.Subscribe<MagazineLoaded>(OnLoaded);
            EventBus.Subscribe<DroneHit>(OnDroneHit);
            EventBus.Subscribe<DroneDestroyed>(OnDroneDestroyed);
            EventBus.Subscribe<ShieldBroken>(OnShieldBroken);
            EventBus.Subscribe<LaserCharging>(OnLaserCharging);
            EventBus.Subscribe<LaserFired>(OnLaserFired);
            EventBus.Subscribe<CarrierReleased>(OnCarrierReleased);
            EventBus.Subscribe<TowerDamaged>(OnTowerDamaged);
            EventBus.Subscribe<PlayerHit>(OnPlayerHit);
            EventBus.Subscribe<LaserBlocked>(OnLaserBlocked);
            EventBus.Subscribe<WaveStarted>(OnWaveStarted);
            EventBus.Subscribe<WaveCleared>(OnWaveCleared);
            EventBus.Subscribe<UpgradeChosen>(OnUpgradeChosen);
            EventBus.Subscribe<RunEnded>(OnRunEnded);
            EventBus.Subscribe<GameStateChanged>(OnStateChanged);
            EventBus.Subscribe<IshemaSpoke>(OnIshema);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<ShotFired>(OnShot);
            EventBus.Unsubscribe<DryFired>(OnDryFire);
            EventBus.Unsubscribe<MagazineEjected>(OnEjected);
            EventBus.Unsubscribe<MagazineLoaded>(OnLoaded);
            EventBus.Unsubscribe<DroneHit>(OnDroneHit);
            EventBus.Unsubscribe<DroneDestroyed>(OnDroneDestroyed);
            EventBus.Unsubscribe<ShieldBroken>(OnShieldBroken);
            EventBus.Unsubscribe<LaserCharging>(OnLaserCharging);
            EventBus.Unsubscribe<LaserFired>(OnLaserFired);
            EventBus.Unsubscribe<CarrierReleased>(OnCarrierReleased);
            EventBus.Unsubscribe<TowerDamaged>(OnTowerDamaged);
            EventBus.Unsubscribe<PlayerHit>(OnPlayerHit);
            EventBus.Unsubscribe<LaserBlocked>(OnLaserBlocked);
            EventBus.Unsubscribe<WaveStarted>(OnWaveStarted);
            EventBus.Unsubscribe<WaveCleared>(OnWaveCleared);
            EventBus.Unsubscribe<UpgradeChosen>(OnUpgradeChosen);
            EventBus.Unsubscribe<RunEnded>(OnRunEnded);
            EventBus.Unsubscribe<GameStateChanged>(OnStateChanged);
            EventBus.Unsubscribe<IshemaSpoke>(OnIshema);
        }

        void Update()
        {
            float step = Time.unscaledDeltaTime;
            musicBase.volume = Mathf.MoveTowards(musicBase.volume, baseTarget, step * 0.3f);
            musicDrive.volume = Mathf.MoveTowards(musicDrive.volume, driveTarget, step * 0.2f);
            musicFilter.cutoffFrequency = Mathf.Lerp(musicFilter.cutoffFrequency, cutoffTarget, step * 3f);
        }

        public void PlayUiClick() => Play(sounds.uiClick, Vector3.zero, 0.6f, false);

        void Play(AudioClip clip, Vector3 position, float volume = 1f, bool spatial = true, float pitchJitter = 0f)
        {
            if (clip == null)
                return;
            var voice = pool.Get(position, Quaternion.identity);
            voice.Play(clip, volume, 1f + Random.Range(-pitchJitter, pitchJitter), spatial);
        }

        void OnShot(ShotFired e) => Play(sounds.shot, e.Position, 0.8f, true, 0.05f);
        void OnDryFire(DryFired e) => Play(sounds.dryFire, e.Position, 0.9f);
        void OnEjected(MagazineEjected e) => Play(sounds.magazineOut, e.Position);

        void OnLoaded(MagazineLoaded e)
        {
            Play(sounds.magazineIn, e.Position);
            if (e.FastReload)
                Play(sounds.fastReload, e.Position, 0.5f, false);
        }

        void OnDroneHit(DroneHit e) => Play(e.Blocked ? sounds.shieldBlock : sounds.hitSpark, e.Position, 0.8f, true, 0.08f);

        void OnDroneDestroyed(DroneDestroyed e)
        {
            var clip = e.Type == DroneType.Carrier ? sounds.bigExplosion : sounds.explosion;
            Play(clip, e.Position, e.ByPlayer ? 1f : 0.8f, true, 0.1f);
        }

        void OnShieldBroken(ShieldBroken e) => Play(sounds.shieldBreak, e.Position);
        void OnLaserCharging(LaserCharging e) => Play(sounds.laserCharge, e.Position, 0.9f);
        void OnLaserFired(LaserFired e) => Play(sounds.laserFire, e.Position);
        void OnCarrierReleased(CarrierReleased e) => Play(sounds.carrierRelease, e.Position);

        void OnTowerDamaged(TowerDamaged e)
        {
            Play(sounds.towerHit, e.Position);
            if (!towerAlarmPlayed && e.Health * 10 < e.MaxHealth * 3)
            {
                towerAlarmPlayed = true;
                Play(sounds.towerAlarm, Vector3.zero, 0.6f, false);
            }
        }

        void OnPlayerHit(PlayerHit e) => Play(sounds.playerHit, Vector3.zero, 1f, false);
        void OnLaserBlocked(LaserBlocked e) => Play(sounds.shieldBlock, e.Position, 1f, true, 0.06f);

        void OnWaveStarted(WaveStarted e)
        {
            towerAlarmPlayed = false;
            Play(sounds.waveStart, Vector3.zero, 0.7f, false);
            // the drive layer comes in harder with every wave
            float intensity = e.Endless ? Mathf.Clamp01(0.4f + e.Wave * 0.06f) : Mathf.Clamp01(e.Wave / (float)Mathf.Max(1, e.TotalWaves));
            driveTarget = musicVolume * Mathf.Lerp(0.35f, 1f, intensity);
            baseTarget = musicVolume;
        }

        void OnWaveCleared(WaveCleared e) => Play(sounds.waveCleared, Vector3.zero, 0.9f, false);
        void OnUpgradeChosen(UpgradeChosen e) => Play(sounds.upgradePicked, Vector3.zero, 0.8f, false);

        void OnRunEnded(RunEnded e)
        {
            Play(e.Result.Won ? sounds.dawn : sounds.gameOver, Vector3.zero, 0.9f, false);
            driveTarget = 0f;
        }

        void OnStateChanged(GameStateChanged e)
        {
            // the upgrade screen is the breakdown: the beat drops out and the pads get muffled
            cutoffTarget = e.State == GameStateId.Upgrade ? 900f : 22000f;
            if (e.State is GameStateId.Upgrade or GameStateId.MainMenu or GameStateId.Tutorial)
                driveTarget = 0f;
            if (e.State == GameStateId.MainMenu)
                baseTarget = musicVolume * 0.7f;
        }

        void OnIshema(IshemaSpoke e) => Play(sounds.ishemaChime, Vector3.zero, 0.4f, false);
    }
}

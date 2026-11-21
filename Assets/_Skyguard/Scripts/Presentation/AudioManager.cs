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
        [SerializeField] AudioSource musicA;
        [SerializeField] AudioSource musicB;
        [SerializeField] AudioLowPassFilter musicFilter;
        [SerializeField] float musicVolume = 0.45f;
        [SerializeField] float crossfadeTime = 2f;

        ObjectPool<PooledAudio> pool;
        // two decks: the one playing now and the one fading out, swapped on every track change
        AudioSource current;
        AudioSource fading;
        float level = 0.7f;
        float cutoffTarget = 22000f;
        bool towerAlarmPlayed;

        void Awake()
        {
            pool = new ObjectPool<PooledAudio>(voicePrefab, voices, transform);
            current = musicA;
            fading = musicB;
            PlayMusic(sounds.musicMenu, true);
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
            float step = Time.unscaledDeltaTime * musicVolume / crossfadeTime;
            current.volume = Mathf.MoveTowards(current.volume, musicVolume * level, step);
            fading.volume = Mathf.MoveTowards(fading.volume, 0f, step);
            if (fading.isPlaying && fading.volume <= 0f)
                fading.Stop();
            musicFilter.cutoffFrequency = Mathf.Lerp(musicFilter.cutoffFrequency, cutoffTarget, Time.unscaledDeltaTime * 3f);
        }

        void PlayMusic(AudioClip clip, bool loop)
        {
            if (clip == null || (current.clip == clip && current.isPlaying))
                return;
            (current, fading) = (fading, current);
            current.clip = clip;
            current.loop = loop;
            current.volume = 0f;
            current.Play();
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
            // the last tower gets the heavier track, and both get louder as the night goes on
            bool finalStretch = e.Endless ? e.Wave >= 6 : e.Wave >= 8;
            PlayMusic(finalStretch ? sounds.musicFinal : sounds.musicBattle, true);
            float intensity = e.Endless ? Mathf.Clamp01(0.4f + e.Wave * 0.06f) : Mathf.Clamp01(e.Wave / (float)Mathf.Max(1, e.TotalWaves));
            level = Mathf.Lerp(0.75f, 1f, intensity);
        }

        void OnWaveCleared(WaveCleared e) => Play(sounds.waveCleared, Vector3.zero, 0.9f, false);
        void OnUpgradeChosen(UpgradeChosen e) => Play(sounds.upgradePicked, Vector3.zero, 0.8f, false);

        void OnRunEnded(RunEnded e)
        {
            Play(e.Result.Won ? sounds.dawn : sounds.gameOver, Vector3.zero, 0.9f, false);
            PlayMusic(e.Result.Won ? sounds.musicDawn : sounds.musicGameOver, false);
            level = 0.85f;
        }

        void OnStateChanged(GameStateChanged e)
        {
            // upgrade cards and the pause menu keep the battle track going, muffled as if through a wall
            cutoffTarget = e.State switch
            {
                GameStateId.Upgrade => 900f,
                GameStateId.Paused => 600f,
                _ => 22000f
            };
            if (e.State is GameStateId.MainMenu or GameStateId.Tutorial)
            {
                PlayMusic(sounds.musicMenu, true);
                level = e.State == GameStateId.Tutorial ? 0.6f : 0.7f;
            }
        }

        void OnIshema(IshemaSpoke e) => Play(sounds.ishemaChime, Vector3.zero, 0.4f, false);
    }
}

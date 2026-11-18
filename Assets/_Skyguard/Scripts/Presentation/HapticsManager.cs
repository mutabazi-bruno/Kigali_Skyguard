using Skyguard.Core;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace Skyguard.Presentation
{
    // a different pulse for each thing, so you can feel a reload click without looking
    public class HapticsManager : MonoBehaviour
    {
        [SerializeField] HapticImpulsePlayer leftHand;
        [SerializeField] HapticImpulsePlayer rightHand;

        void OnEnable()
        {
            EventBus.Subscribe<ShotFired>(OnShot);
            EventBus.Subscribe<DryFired>(OnDryFire);
            EventBus.Subscribe<MagazineLoaded>(OnLoaded);
            EventBus.Subscribe<MagazineEjected>(OnEjected);
            EventBus.Subscribe<PlayerHit>(OnPlayerHit);
            EventBus.Subscribe<LaserBlocked>(OnLaserBlocked);
            EventBus.Subscribe<WaveCleared>(OnWaveCleared);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<ShotFired>(OnShot);
            EventBus.Unsubscribe<DryFired>(OnDryFire);
            EventBus.Unsubscribe<MagazineLoaded>(OnLoaded);
            EventBus.Unsubscribe<MagazineEjected>(OnEjected);
            EventBus.Unsubscribe<PlayerHit>(OnPlayerHit);
            EventBus.Unsubscribe<LaserBlocked>(OnLaserBlocked);
            EventBus.Unsubscribe<WaveCleared>(OnWaveCleared);
        }

        public void Pulse(bool leftHand, float strength, float seconds) =>
            (leftHand ? this.leftHand : rightHand).SendHapticImpulse(strength, seconds);

        void PulseBoth(float strength, float seconds)
        {
            Pulse(true, strength, seconds);
            Pulse(false, strength, seconds);
        }

        void OnShot(ShotFired e) => Pulse(e.LeftHand, 0.45f, 0.05f);
        void OnDryFire(DryFired e) => Pulse(e.LeftHand, 0.15f, 0.02f);
        void OnEjected(MagazineEjected e) => Pulse(e.LeftHand, 0.3f, 0.06f);

        // both hands feel the click: one is holding the gun, the other just pushed the magazine in
        void OnLoaded(MagazineLoaded e) => PulseBoth(0.7f, 0.08f);
        void OnPlayerHit(PlayerHit e) => PulseBoth(1f, 0.25f);
        void OnLaserBlocked(LaserBlocked e) => Pulse(e.LeftHand, 0.8f, 0.12f);
        void OnWaveCleared(WaveCleared e) => PulseBoth(0.35f, 0.4f);
    }
}

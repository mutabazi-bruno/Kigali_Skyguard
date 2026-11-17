using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Gameplay
{
    // capsule that follows the headset around. lasers hit this, so leaning and ducking really dodge them
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerShield : MonoBehaviour, IDamageable
    {
        [SerializeField] Transform head;
        [SerializeField] int maxPoints = 3;
        [SerializeField] float graceTime = 0.6f;

        CapsuleCollider capsule;
        float safeUntil;

        public int Points { get; private set; }
        public int MaxPoints => maxPoints;
        public bool IsDepleted => Points <= 0;

        void Awake()
        {
            capsule = GetComponent<CapsuleCollider>();
            Points = maxPoints;
        }

        void LateUpdate()
        {
            // stay upright even when the head tilts, and hang the capsule down over the chest
            var yaw = Quaternion.Euler(0f, head.eulerAngles.y, 0f);
            transform.SetPositionAndRotation(head.position + Vector3.down * (capsule.height * 0.5f - 0.12f), yaw);
        }

        public void Refill()
        {
            Points = maxPoints;
            EventBus.Publish(new PlayerShieldRefilled { Points = Points });
        }

        public void TakeDamage(DamageInfo hit)
        {
            if (hit.Source != DamageSource.Drone || IsDepleted || Time.time < safeUntil)
                return;

            Points--;
            safeUntil = Time.time + graceTime;
            EventBus.Publish(new PlayerHit { Position = hit.Point, ShieldLeft = Points });
        }
    }
}

using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Drones
{
    public class CarrierWeakPoint : MonoBehaviour, IDamageable
    {
        [SerializeField] Renderer glow;
        [SerializeField] Material liveMaterial;
        [SerializeField] Material brokenMaterial;

        CarrierDrone carrier;
        int hitsLeft;

        public bool IsBroken => hitsLeft <= 0;

        public void Arm(CarrierDrone owner, int hits)
        {
            carrier = owner;
            hitsLeft = hits;
            glow.sharedMaterial = liveMaterial;
        }

        public void TakeDamage(DamageInfo hit)
        {
            if (IsBroken || !carrier.IsAlive || hit.Source != DamageSource.Player)
                return;

            hitsLeft--;
            EventBus.Publish(new DroneHit { Position = hit.Point, Type = DroneType.Carrier });
            if (!IsBroken)
                return;

            glow.sharedMaterial = brokenMaterial;
            EventBus.Publish(new ShieldBroken { Position = transform.position });
            carrier.OnWeakPointBroken(hit);
        }
    }
}

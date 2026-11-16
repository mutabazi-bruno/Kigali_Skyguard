using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Weapons
{
    // slow on purpose (6 m/s) so the player can see it coming and lean out of the way
    public class DroneLaser : Projectile
    {
        protected override bool OnImpact(RaycastHit hit, Vector3 heading)
        {
            var target = hit.collider.GetComponentInParent<IDamageable>();
            target?.TakeDamage(new DamageInfo
            {
                Amount = damage,
                Point = hit.point,
                Direction = heading,
                Source = DamageSource.Drone
            });
            return true;
        }
    }
}

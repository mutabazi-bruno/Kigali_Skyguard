using Skyguard.Core;
using Skyguard.Drones;
using UnityEngine;

namespace Skyguard.Weapons
{
    public class PlayerBolt : Projectile
    {
        [SerializeField] Gradient normalTrail;
        [SerializeField] Gradient piercingTrail;

        int pierceLeft;
        bool hitEnemy;

        public int PierceCount
        {
            get => pierceLeft;
            set
            {
                pierceLeft = value;
                if (Trail)
                    Trail.colorGradient = value > 0 ? piercingTrail : normalTrail;
            }
        }

        public override void OnSpawn()
        {
            PierceCount = 0;
            hitEnemy = false;
        }

        protected override bool OnImpact(RaycastHit hit, Vector3 heading)
        {
            var target = hit.collider.GetComponentInParent<IDamageable>();
            if (target == null)
                return true;

            target.TakeDamage(new DamageInfo
            {
                Amount = damage,
                Point = hit.point,
                Direction = heading,
                Source = DamageSource.Player
            });

            if (hit.collider.GetComponentInParent<Drone>() != null)
                hitEnemy = true;

            if (pierceLeft <= 0)
                return true;

            pierceLeft--;
            return false;
        }

        // one shot = one result, even if a piercing bolt went through two drones
        protected override void OnFinished() => EventBus.Publish(new ShotResolved { HitEnemy = hitEnemy });
    }
}

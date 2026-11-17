using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Drones
{
    // keeps a hex shield turned towards the player. hit it from the side, or break the shield first
    public class ShieldedDrone : Drone
    {
        [SerializeField] GameObject shield;
        [SerializeField] float crashDistance = 1.6f;

        int shieldHits;
        Vector3 aimOffset;

        bool ShieldUp => shieldHits > 0;

        protected override void OnInit()
        {
            shieldHits = config.shieldHits;
            shield.SetActive(true);
            aimOffset = Random.insideUnitSphere;

            // start facing the way it's flying. the slow turn towards the player is the window to flank it
            transform.rotation = Quaternion.LookRotation(context.Tower.AimPoint - transform.position);
        }

        protected override void Behave(float deltaTime)
        {
            var target = context.Tower.AimPoint + aimOffset;
            if (Vector3.Distance(transform.position, target) < crashDistance)
            {
                Crash();
                return;
            }

            FlyTowards(target, deltaTime);
            TurnTowards(context.PlayerHead.position, config.turnSpeed, deltaTime);
        }

        public override void TakeDamage(DamageInfo hit)
        {
            if (!IsAlive || hit.Source != DamageSource.Player)
                return;

            bool fromFront = Vector3.Angle(transform.forward, -hit.Direction) < config.blockAngle;
            if (ShieldUp && fromFront)
            {
                shieldHits--;
                EventBus.Publish(new DroneHit { Position = hit.Point, Type = Type, Blocked = true });
                if (!ShieldUp)
                {
                    shield.SetActive(false);
                    EventBus.Publish(new ShieldBroken { Position = shield.transform.position });
                }
                return;
            }

            ApplyDamage(hit);
        }
    }
}

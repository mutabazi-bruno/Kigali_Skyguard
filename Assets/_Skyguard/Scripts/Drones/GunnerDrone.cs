using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Drones
{
    // hangs back 8-15 m from the player, charges for 1.5 s, then fires a slow laser at their head
    public class GunnerDrone : Drone
    {
        [SerializeField] Transform muzzle;
        [SerializeField] float strafeSpeed = 6f;

        enum Phase { Approach, Hover, Charge }

        Phase phase;
        Vector3 hoverPoint;
        float timer;
        float strafeDirection;

        public bool IsCharging => IsAlive && phase == Phase.Charge;
        public float ChargeProgress => IsCharging ? 1f - timer / config.chargeTime : 0f;

        protected override void OnInit()
        {
            phase = Phase.Approach;
            hoverPoint = PickHoverPoint();
            strafeDirection = Random.value < 0.5f ? -1f : 1f;
        }

        Vector3 PickHoverPoint()
        {
            var head = context.PlayerHead.position;
            var away = transform.position - head;
            away.y = 0f;
            float distance = Random.Range(config.hoverDistance.x, config.hoverDistance.y);
            return head + away.normalized * distance + Vector3.up * Random.Range(1f, 4f);
        }

        protected override void Behave(float deltaTime)
        {
            var head = context.PlayerHead.position;
            TurnTowards(head, 120f, deltaTime);

            switch (phase)
            {
                case Phase.Approach:
                    FlyTowards(hoverPoint, deltaTime);
                    if ((transform.position - hoverPoint).sqrMagnitude < 0.25f)
                    {
                        phase = Phase.Hover;
                        timer = config.fireInterval * 0.5f;
                    }
                    break;

                case Phase.Hover:
                    // slow orbit around the player so it isn't a sitting duck
                    var offset = transform.position - head;
                    transform.position = head + Quaternion.AngleAxis(strafeDirection * strafeSpeed * deltaTime, Vector3.up) * offset;
                    timer -= deltaTime;
                    if (timer <= 0f)
                    {
                        phase = Phase.Charge;
                        timer = config.chargeTime;
                        EventBus.Publish(new LaserCharging { Position = muzzle.position, Duration = config.chargeTime });
                    }
                    break;

                case Phase.Charge:
                    timer -= deltaTime;
                    if (timer <= 0f)
                    {
                        Fire(head);
                        phase = Phase.Hover;
                        timer = config.fireInterval;
                    }
                    break;
            }
        }

        void Fire(Vector3 head)
        {
            // aim a little under the eyes so ducking actually gets you out of the way
            var aim = head + Vector3.down * 0.2f - muzzle.position;
            context.Lasers.Launch(muzzle.position, aim, context.LaserSpeedMultiplier);
            EventBus.Publish(new LaserFired { Position = muzzle.position });
        }
    }
}

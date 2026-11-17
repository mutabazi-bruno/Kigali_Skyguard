using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Drones
{
    // wave 10. big and slow, circles the tower dropping scouts until its glowing weak points are gone
    public class CarrierDrone : Drone
    {
        [SerializeField] Transform baySpawn;
        [SerializeField] Transform hatch;
        [SerializeField] CarrierWeakPoint[] weakPoints;
        [SerializeField] float stationDistance = 30f;
        [SerializeField] float stationHeight = 14f;
        [SerializeField] float orbitSpeed = 4f;

        Vector3 station;
        bool onStation;
        float releaseTimer;
        float hatchOpenFor;
        int pointsLeft;
        Quaternion hatchClosed;

        protected override void Awake()
        {
            base.Awake();
            hatchClosed = hatch.localRotation;
        }

        protected override void OnInit()
        {
            pointsLeft = weakPoints.Length;
            foreach (var point in weakPoints)
                point.Arm(this, config.weakPointHits);

            var tower = context.Tower.transform.position;
            var away = transform.position - tower;
            away.y = 0f;
            station = tower + away.normalized * stationDistance + Vector3.up * stationHeight;
            onStation = false;
            releaseTimer = 3f;
        }

        protected override void Behave(float deltaTime)
        {
            var tower = context.Tower.transform.position;

            if (!onStation)
            {
                FlyTowards(station, deltaTime);
                onStation = (transform.position - station).sqrMagnitude < 1f;
            }
            else
            {
                var offset = transform.position - tower;
                transform.position = tower + Quaternion.AngleAxis(orbitSpeed * deltaTime, Vector3.up) * offset;
            }

            TurnTowards(context.PlayerHead.position, 15f, deltaTime);

            releaseTimer -= deltaTime;
            if (releaseTimer <= 0f)
            {
                ReleaseScouts();
                releaseTimer = config.releaseInterval;
            }

            hatchOpenFor -= deltaTime;
            var hatchGoal = hatchOpenFor > 0f ? hatchClosed * Quaternion.Euler(70f, 0f, 0f) : hatchClosed;
            hatch.localRotation = Quaternion.RotateTowards(hatch.localRotation, hatchGoal, 180f * deltaTime);
        }

        void ReleaseScouts()
        {
            hatchOpenFor = 1.5f;
            for (int i = 0; i < config.releaseCount; i++)
            {
                var position = baySpawn.position + Random.insideUnitSphere * 1.5f;
                context.Factory.Create(DroneType.Scout, position, transform.rotation, context);
            }
            EventBus.Publish(new CarrierReleased { Position = baySpawn.position });
        }

        // the hull is armour, only the weak points take damage
        public override void TakeDamage(DamageInfo hit)
        {
            if (IsAlive && hit.Source == DamageSource.Player)
                EventBus.Publish(new DroneHit { Position = hit.Point, Type = Type, Blocked = true });
        }

        public void OnWeakPointBroken(DamageInfo hit)
        {
            pointsLeft--;
            if (pointsLeft <= 0)
                Die(hit.Direction);
        }
    }
}

using UnityEngine;

namespace Skyguard.Drones
{
    // fast and fragile, dives straight at the tower
    public class ScoutDrone : Drone
    {
        [SerializeField] float crashDistance = 1.3f;

        Vector3 aimOffset;
        float weavePhase;

        protected override void OnInit()
        {
            // spread them over the tower so they don't all hit the exact same point
            aimOffset = Random.insideUnitSphere * 1.2f;
            weavePhase = Random.value * 10f;
        }

        protected override void Behave(float deltaTime)
        {
            var target = context.Tower.AimPoint + aimOffset;
            var toTarget = target - transform.position;
            float distance = toTarget.magnitude;

            if (distance < crashDistance)
            {
                Crash();
                return;
            }

            // a lazy weave that fades out on the final approach
            var side = Vector3.Cross(toTarget / distance, Vector3.up);
            float weave = Mathf.Sin(Time.time * 2.5f + weavePhase) * Mathf.Clamp01(distance / 20f) * 0.5f;
            var heading = (toTarget / distance + side * weave).normalized;

            transform.position += heading * (Speed * deltaTime);
            TurnTowards(transform.position + heading, 360f, deltaTime);
        }
    }
}

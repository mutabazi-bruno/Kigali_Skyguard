using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Gameplay
{
    // the energy disc on the off-hand gauntlet. a gunner laser that hits it stops there,
    // so raising your arm at the right moment saves a shield point
    public class ShieldBlock : MonoBehaviour, IDamageable
    {
        [SerializeField] Transform leftController;

        public void TakeDamage(DamageInfo hit)
        {
            if (hit.Source != DamageSource.Drone)
                return;

            // the gauntlet swaps wrists in left-handed mode, so check where it is right now
            EventBus.Publish(new LaserBlocked { Position = hit.Point, LeftHand = transform.IsChildOf(leftController) });
        }
    }
}

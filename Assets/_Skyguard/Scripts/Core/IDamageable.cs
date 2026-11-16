using UnityEngine;

namespace Skyguard.Core
{
    public enum DamageSource { Player, Drone }

    public struct DamageInfo
    {
        public int Amount;
        public Vector3 Point;
        public Vector3 Direction;
        public DamageSource Source;
    }

    public interface IDamageable
    {
        void TakeDamage(DamageInfo hit);
    }
}

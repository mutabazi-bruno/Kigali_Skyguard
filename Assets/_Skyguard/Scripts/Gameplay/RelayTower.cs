using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Gameplay
{
    public class RelayTower : MonoBehaviour, IDamageable
    {
        [SerializeField] int maxHealth = 100;
        [SerializeField] Transform hitCenter;

        public int Health { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsDestroyed => Health <= 0;
        public Vector3 AimPoint => hitCenter.position;

        void Awake() => Health = maxHealth;

        public void ResetHealth() => SetHealth(maxHealth);

        public void SetHealth(int value)
        {
            Health = Mathf.Clamp(value, 0, maxHealth);
            EventBus.Publish(new TowerRepaired { Health = Health, MaxHealth = maxHealth });
        }

        public void Repair(int amount) => SetHealth(Health + amount);

        public void TakeDamage(DamageInfo hit)
        {
            // the player's own bolts just splash off it
            if (hit.Source != DamageSource.Drone || IsDestroyed)
                return;

            Health = Mathf.Max(0, Health - hit.Amount);
            EventBus.Publish(new TowerDamaged { Position = hit.Point, Health = Health, MaxHealth = maxHealth });
        }
    }
}

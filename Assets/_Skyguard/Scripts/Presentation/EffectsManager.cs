using System.Collections.Generic;
using Skyguard.Core;
using Skyguard.Drones;
using UnityEngine;

namespace Skyguard.Presentation
{
    public class EffectsManager : MonoBehaviour
    {
        [SerializeField] PooledEffect muzzleFlash;
        [SerializeField] PooledEffect sparks;
        [SerializeField] PooledEffect shieldSpark;
        [SerializeField] PooledEffect explosion;
        [SerializeField] PooledEffect bigExplosion;
        [SerializeField] PooledEffect shieldShatter;
        [SerializeField] PooledEffect towerHit;
        [SerializeField] PooledEffect magazineClick;

        readonly Dictionary<PooledEffect, ObjectPool<PooledEffect>> pools = new();

        void Awake()
        {
            Prepare(muzzleFlash, 6);
            Prepare(sparks, 12);
            Prepare(shieldSpark, 8);
            Prepare(explosion, 10);
            Prepare(bigExplosion, 1);
            Prepare(shieldShatter, 3);
            Prepare(towerHit, 4);
            Prepare(magazineClick, 2);
        }

        void Prepare(PooledEffect prefab, int count) => pools[prefab] = new ObjectPool<PooledEffect>(prefab, count, transform);

        void Spawn(PooledEffect prefab, Vector3 position, Vector3 facing)
        {
            var rotation = facing.sqrMagnitude > 0.001f ? Quaternion.LookRotation(facing) : Quaternion.identity;
            pools[prefab].Get(position, rotation);
        }

        void OnEnable()
        {
            EventBus.Subscribe<ShotFired>(OnShot);
            EventBus.Subscribe<DroneHit>(OnDroneHit);
            EventBus.Subscribe<DroneDestroyed>(OnDroneDestroyed);
            EventBus.Subscribe<ShieldBroken>(OnShieldBroken);
            EventBus.Subscribe<TowerDamaged>(OnTowerDamaged);
            EventBus.Subscribe<MagazineLoaded>(OnLoaded);
            EventBus.Subscribe<LaserBlocked>(OnLaserBlocked);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<ShotFired>(OnShot);
            EventBus.Unsubscribe<DroneHit>(OnDroneHit);
            EventBus.Unsubscribe<DroneDestroyed>(OnDroneDestroyed);
            EventBus.Unsubscribe<ShieldBroken>(OnShieldBroken);
            EventBus.Unsubscribe<TowerDamaged>(OnTowerDamaged);
            EventBus.Unsubscribe<MagazineLoaded>(OnLoaded);
            EventBus.Unsubscribe<LaserBlocked>(OnLaserBlocked);
        }

        void OnShot(ShotFired e) => Spawn(muzzleFlash, e.Position, e.Direction);
        void OnDroneHit(DroneHit e) => Spawn(e.Blocked ? shieldSpark : sparks, e.Position, Vector3.up);
        void OnDroneDestroyed(DroneDestroyed e) => Spawn(e.Type == DroneType.Carrier ? bigExplosion : explosion, e.Position, Vector3.up);
        void OnShieldBroken(ShieldBroken e) => Spawn(shieldShatter, e.Position, Vector3.up);
        void OnTowerDamaged(TowerDamaged e) => Spawn(towerHit, e.Position, Vector3.up);
        void OnLoaded(MagazineLoaded e) => Spawn(magazineClick, e.Position, Vector3.up);
        void OnLaserBlocked(LaserBlocked e) => Spawn(shieldSpark, e.Position, Vector3.up);
    }
}

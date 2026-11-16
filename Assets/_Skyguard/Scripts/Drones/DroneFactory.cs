using System;
using System.Collections.Generic;
using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Drones
{
    // the only thing that knows which prefab goes with which drone type
    public class DroneFactory : MonoBehaviour
    {
        [SerializeField] Drone[] prefabs;
        [SerializeField] int prewarmPerType = 8;

        readonly Dictionary<DroneType, ObjectPool<Drone>> pools = new();

        // the wave manager listens here, so scouts dropped by a carrier get ticked like any other drone
        public event Action<Drone> Spawned;

        void Awake()
        {
            foreach (var prefab in prefabs)
            {
                int prewarm = prefab.Type == DroneType.Carrier ? 1 : prewarmPerType;
                pools[prefab.Type] = new ObjectPool<Drone>(prefab, prewarm, transform);
            }
        }

        public Drone Create(DroneType type, Vector3 position, Quaternion rotation, DroneContext context)
        {
            var drone = pools[type].Get(position, rotation);
            drone.Init(context);
            Spawned?.Invoke(drone);
            return drone;
        }
    }
}

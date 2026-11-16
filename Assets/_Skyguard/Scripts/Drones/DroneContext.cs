using Skyguard.Gameplay;
using Skyguard.Weapons;
using UnityEngine;

namespace Skyguard.Drones
{
    // what a drone is allowed to know about the world. one shared instance, owned by the wave manager
    public class DroneContext
    {
        public RelayTower Tower;
        public Transform PlayerHead;
        public ProjectileLauncher Lasers;
        public DroneFactory Factory;
        public float SpeedMultiplier = 1f;
        public float LaserSpeedMultiplier = 1f;
    }
}

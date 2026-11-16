using UnityEngine;

namespace Skyguard.Drones
{
    public enum DroneType { Scout, Gunner, Shielded, Carrier }

    // every number a playtest might want to change lives here, not in code
    [CreateAssetMenu(menuName = "Skyguard/Drone Config")]
    public class DroneConfig : ScriptableObject
    {
        public DroneType type;
        public int maxHealth = 1;
        public float speed = 7f;
        public int points = 100;
        public int towerDamage = 10;

        [Header("Gunner")]
        public Vector2 hoverDistance = new(8f, 15f);
        public float chargeTime = 1.5f;
        public float fireInterval = 3.5f;

        [Header("Shielded")]
        public int shieldHits = 4;
        public float turnSpeed = 25f;
        public float blockAngle = 60f;

        [Header("Carrier")]
        public int weakPointHits = 6;
        public float releaseInterval = 8f;
        public int releaseCount = 3;
    }
}

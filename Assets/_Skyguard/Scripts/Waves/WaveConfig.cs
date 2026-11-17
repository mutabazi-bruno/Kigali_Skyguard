using System;
using System.Collections.Generic;
using Skyguard.Drones;
using UnityEngine;

namespace Skyguard.Waves
{
    public enum SpawnSide { Front, Sides, Anywhere }

    [Serializable]
    public class SpawnEntry
    {
        public DroneType type;
        public int count = 1;
        public float startTime;
        public float interval = 2f;
        public SpawnSide side;
    }

    [CreateAssetMenu(menuName = "Skyguard/Wave Config")]
    public class WaveConfig : ScriptableObject
    {
        public string ishemaLine = "wave_next";
        public float speedMultiplier = 1f;
        public List<SpawnEntry> spawns = new();
    }
}

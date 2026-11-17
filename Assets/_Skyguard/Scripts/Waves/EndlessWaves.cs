using Skyguard.Drones;
using UnityEngine;

namespace Skyguard.Waves
{
    // endless mode builds each round on the fly, a bit bigger and faster every time
    public static class EndlessWaves
    {
        public static WaveConfig Build(int round)
        {
            var wave = ScriptableObject.CreateInstance<WaveConfig>();
            wave.name = $"Endless {round}";
            wave.ishemaLine = round == 1 ? "endless" : "wave_next";
            wave.speedMultiplier = Mathf.Min(1f + 0.04f * (round - 1), 1.6f);

            float interval = Mathf.Max(0.8f, 2.2f - 0.1f * round);
            Add(wave, DroneType.Scout, 5 + 2 * round, 1f, interval, round < 3 ? SpawnSide.Front : SpawnSide.Anywhere);
            if (round >= 2)
                Add(wave, DroneType.Gunner, 1 + round / 2, 6f, 7f, SpawnSide.Front);
            if (round >= 3)
                Add(wave, DroneType.Shielded, round / 2, 10f, 6f, SpawnSide.Sides);
            if (round % 5 == 0)
                Add(wave, DroneType.Carrier, 1, 4f, 1f, SpawnSide.Front);
            return wave;
        }

        static void Add(WaveConfig wave, DroneType type, int count, float start, float interval, SpawnSide side)
        {
            wave.spawns.Add(new SpawnEntry { type = type, count = count, startTime = start, interval = interval, side = side });
        }
    }
}

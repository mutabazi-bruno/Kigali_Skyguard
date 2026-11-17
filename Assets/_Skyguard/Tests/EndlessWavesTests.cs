using System.Linq;
using NUnit.Framework;
using Skyguard.Drones;
using Skyguard.Waves;
using UnityEngine;

namespace Skyguard.Tests
{
    public class EndlessWavesTests
    {
        static int Count(WaveConfig wave) => wave.spawns.Sum(g => g.count);

        [Test]
        public void LaterRoundsSendMoreDrones()
        {
            var early = EndlessWaves.Build(1);
            var late = EndlessWaves.Build(8);
            Assert.Greater(Count(late), Count(early));
            Object.DestroyImmediate(early);
            Object.DestroyImmediate(late);
        }

        [Test]
        public void EveryFifthRoundHasACarrier()
        {
            var fifth = EndlessWaves.Build(5);
            var sixth = EndlessWaves.Build(6);
            Assert.IsTrue(fifth.spawns.Any(g => g.type == DroneType.Carrier));
            Assert.IsFalse(sixth.spawns.Any(g => g.type == DroneType.Carrier));
            Object.DestroyImmediate(fifth);
            Object.DestroyImmediate(sixth);
        }

        [Test]
        public void SpeedIsCapped()
        {
            var wave = EndlessWaves.Build(100);
            Assert.LessOrEqual(wave.speedMultiplier, 1.6f);
            Object.DestroyImmediate(wave);
        }
    }
}

using System.Collections.Generic;
using Skyguard.Drones;
using Skyguard.Gameplay;
using Skyguard.Weapons;
using UnityEngine;

namespace Skyguard.Waves
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] WaveConfig[] campaign;
        [SerializeField] DroneFactory factory;
        [SerializeField] ProjectileLauncher lasers;
        [SerializeField] Transform playerHead;

        struct PendingSpawn
        {
            public float Time;
            public DroneType Type;
            public SpawnSide Side;
        }

        readonly List<Drone> active = new();
        readonly List<PendingSpawn> schedule = new();
        readonly DroneContext context = new();
        SpawnArea area;
        float clock;

        public int CampaignLength => campaign.Length;
        public float DifficultySpeed { get; set; } = 1f;

        public float LaserSpeedMultiplier
        {
            get => context.LaserSpeedMultiplier;
            set => context.LaserSpeedMultiplier = value;
        }

        public int AliveCount
        {
            get
            {
                int alive = 0;
                foreach (var drone in active)
                {
                    if (drone.IsAlive)
                        alive++;
                }
                return alive;
            }
        }

        public bool IsCleared => schedule.Count == 0 && AliveCount == 0;
        public IReadOnlyList<Drone> Active => active;

        void Awake()
        {
            context.Lasers = lasers;
            context.Factory = factory;
            context.PlayerHead = playerHead;
            factory.Spawned += active.Add;
        }

        public void SetBattlefield(RelayTower tower, SpawnArea spawnArea)
        {
            context.Tower = tower;
            area = spawnArea;
        }

        public WaveConfig GetWave(int number, bool endless) =>
            endless ? EndlessWaves.Build(number) : campaign[Mathf.Clamp(number - 1, 0, campaign.Length - 1)];

        public void StartWave(WaveConfig wave)
        {
            Clear();
            clock = 0f;
            context.SpeedMultiplier = DifficultySpeed * wave.speedMultiplier;

            foreach (var entry in wave.spawns)
            {
                for (int i = 0; i < entry.count; i++)
                    schedule.Add(new PendingSpawn { Time = entry.startTime + i * entry.interval, Type = entry.type, Side = entry.side });
            }
            schedule.Sort((a, b) => a.Time.CompareTo(b.Time));
        }

        public void Tick(float deltaTime)
        {
            clock += deltaTime;
            while (schedule.Count > 0 && schedule[0].Time <= clock)
            {
                Spawn(schedule[0]);
                schedule.RemoveAt(0);
            }

            // backwards, because drones leave the list once they're back in the pool
            // and a carrier can add new scouts to the end while we're looping
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var drone = active[i];
                if (drone.gameObject.activeSelf)
                    drone.Tick(deltaTime);
                if (!drone.gameObject.activeSelf)
                    active.RemoveAt(i);
            }
        }

        public void Clear()
        {
            schedule.Clear();
            for (int i = active.Count - 1; i >= 0; i--)
                active[i].Despawn();
            active.Clear();
            lasers.RecallAll();
        }

        void Spawn(PendingSpawn pending)
        {
            var position = area.PickPoint(pending.Side);
            var facing = Quaternion.LookRotation(context.Tower.AimPoint - position);
            factory.Create(pending.Type, position, facing, context);
        }
    }
}

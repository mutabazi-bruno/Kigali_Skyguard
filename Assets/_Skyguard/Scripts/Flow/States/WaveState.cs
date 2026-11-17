using Skyguard.Core;
using Skyguard.Data;
using UnityEngine;

namespace Skyguard.Flow
{
    public class WaveState : IGameState
    {
        readonly GameManager game;
        bool finished;

        public WaveState(GameManager game) => this.game = game;

        public GameStateId Id => GameStateId.Wave;

        public void Enter()
        {
            bool endless = game.Mode == GameMode.Endless;
            var wave = game.Waves.GetWave(game.Wave, endless);

            game.SnapshotWaveStart();
            game.PlayerShield.Refill();
            game.Waves.StartWave(wave);
            EventBus.Publish(new WaveStarted { Wave = game.Wave, TotalWaves = game.TotalWaves, Endless = endless });
            EventBus.Publish(new IshemaSpoke { Key = wave.ishemaLine });

            // endless waves are built at runtime, nothing else holds on to them
            if (endless)
                Object.Destroy(wave);
        }

        public void Tick(float deltaTime)
        {
            if (finished)
                return;

            game.Waves.Tick(deltaTime);

            if (game.Tower.IsDestroyed || game.PlayerShield.IsDepleted)
            {
                finished = true;
                game.EndRun(false);
            }
            else if (game.Waves.IsCleared)
            {
                finished = true;
                game.OnWaveCleared();
            }
        }

        public void Exit() { }
    }
}

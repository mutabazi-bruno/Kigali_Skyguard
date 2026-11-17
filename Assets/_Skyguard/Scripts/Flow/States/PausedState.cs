using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Flow
{
    public class PausedState : IGameState
    {
        public GameStateId Id => GameStateId.Paused;

        public void Enter()
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }

        public void Tick(float deltaTime) { }

        public void Exit()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}

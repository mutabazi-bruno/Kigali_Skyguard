using Skyguard.Core;
using UnityEngine;

namespace Skyguard.Flow
{
    public class MainMenuState : IGameState
    {
        readonly GameManager game;

        public MainMenuState(GameManager game) => this.game = game;

        public GameStateId Id => GameStateId.MainMenu;

        public void Enter()
        {
            Time.timeScale = 1f;
            game.ResetToMenu();
        }

        public void Tick(float deltaTime) { }
        public void Exit() { }
    }
}

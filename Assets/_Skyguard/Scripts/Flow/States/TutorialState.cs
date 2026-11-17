using Skyguard.Core;

namespace Skyguard.Flow
{
    public class TutorialState : IGameState
    {
        readonly GameManager game;

        public TutorialState(GameManager game) => this.game = game;

        public GameStateId Id => GameStateId.Tutorial;

        public void Enter() => game.Tutorial.Begin(game.Rooftops.Current.PracticeTargetAnchor, game.OnTutorialFinished);
        public void Tick(float deltaTime) { }
        public void Exit() => game.Tutorial.Stop();
    }
}

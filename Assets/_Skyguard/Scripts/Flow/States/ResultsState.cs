using Skyguard.Core;

namespace Skyguard.Flow
{
    public class ResultsState : IGameState
    {
        readonly GameManager game;
        readonly bool won;

        public ResultsState(GameManager game, bool won)
        {
            this.game = game;
            this.won = won;
        }

        public GameStateId Id => GameStateId.Results;

        public void Enter()
        {
            game.Waves.Clear();
            game.Upgrades.Cancel();
            game.Holster.ReturnNow();

            // the score is saved before the results screen shows, win or lose
            var result = game.FinishRun(won);
            EventBus.Publish(new RunEnded { Result = result });
            EventBus.Publish(new IshemaSpoke { Key = won ? "dawn" : "game_over" });
        }

        public void Tick(float deltaTime) { }
        public void Exit() { }
    }
}

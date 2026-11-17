using Skyguard.Core;

namespace Skyguard.Flow
{
    public class UpgradeState : IGameState
    {
        readonly GameManager game;

        public UpgradeState(GameManager game) => this.game = game;

        public GameStateId Id => GameStateId.Upgrade;

        public void Enter()
        {
            EventBus.Publish(new IshemaSpoke { Key = "wave_clear" });
            game.Upgrades.Offer(game.UpgradeContext, game.Rooftops.Current.CardAnchor, game.OnUpgradePicked);
        }

        public void Tick(float deltaTime) { }
        public void Exit() => game.Upgrades.Cancel();
    }
}

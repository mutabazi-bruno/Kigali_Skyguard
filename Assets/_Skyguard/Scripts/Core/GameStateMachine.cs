namespace Skyguard.Core
{
    public enum GameStateId { MainMenu, Tutorial, Wave, Upgrade, Paused, Results }

    public interface IGameState
    {
        GameStateId Id { get; }
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }

    public class GameStateMachine
    {
        IGameState underOverlay;

        public IGameState Current { get; private set; }
        public bool HasOverlay => underOverlay != null;

        public void ChangeState(IGameState next)
        {
            if (HasOverlay)
            {
                Current.Exit();
                Current = underOverlay;
                underOverlay = null;
            }

            Current?.Exit();
            Current = next;
            Current.Enter();
            Announce();
        }

        // pause sits on top of the running state and hands control back without re-entering it
        public void PushOverlay(IGameState overlay)
        {
            if (HasOverlay)
                return;
            underOverlay = Current;
            Current = overlay;
            Current.Enter();
            Announce();
        }

        public void PopOverlay()
        {
            if (!HasOverlay)
                return;
            Current.Exit();
            Current = underOverlay;
            underOverlay = null;
            Announce();
        }

        public void Tick(float deltaTime) => Current?.Tick(deltaTime);

        void Announce() => EventBus.Publish(new GameStateChanged { State = Current.Id });
    }
}

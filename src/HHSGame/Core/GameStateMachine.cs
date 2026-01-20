namespace HHSGame.Core
{
    public enum GameStateType
    {
        Setup,
        Exploration,
        Combat,
        Dialogue,
        Menu,
        Inventory,
        GameOver
    }

    public class GameStateChangedEventArgs(GameStateType previous, GameStateType current) : EventArgs
    {
        public GameStateType Previous { get; } = previous;
        public GameStateType Current { get; } = current;
    }

    public class GameStateMachine
    {
        private readonly Dictionary<GameStateType, HashSet<GameStateType>> transitions;

        public GameStateType CurrentState { get; private set; } = GameStateType.Setup;

        public event EventHandler<GameStateChangedEventArgs>? StateChanged;

        public GameStateMachine()
        {
            transitions = new()
            {
                [GameStateType.Setup] = [GameStateType.Exploration, GameStateType.Menu],
                [GameStateType.Exploration] =
                [
                    GameStateType.Combat,
                    GameStateType.Dialogue,
                    GameStateType.Menu,
                    GameStateType.Inventory
                ],
                [GameStateType.Combat] = [GameStateType.Exploration, GameStateType.Menu, GameStateType.Inventory],
                [GameStateType.Dialogue] = [GameStateType.Exploration, GameStateType.Menu],
                [GameStateType.Menu] = [GameStateType.Exploration],
                [GameStateType.Inventory] = [GameStateType.Exploration, GameStateType.Combat],
                [GameStateType.GameOver] = []
            };
        }

        public bool CanTransitionTo(GameStateType state)
        {
            if (state == CurrentState)
            {
                return true;
            }

            if (state == GameStateType.GameOver)
            {
                return true;
            }

            return transitions.TryGetValue(CurrentState, out HashSet<GameStateType>? allowed)
                && allowed.Contains(state);
        }

        public bool TryChangeState(GameStateType state)
        {
            if (!CanTransitionTo(state))
            {
                return false;
            }

            if (state == CurrentState)
            {
                return true;
            }

            GameStateType previous = CurrentState;
            CurrentState = state;
            StateChanged?.Invoke(this, new GameStateChangedEventArgs(previous, state));
            return true;
        }
    }
}

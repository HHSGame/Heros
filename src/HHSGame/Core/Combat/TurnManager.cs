

namespace HHSGame.Core.Combat
{
    public class TurnManager
    {
        private TurnState currentTurnState = TurnState.PlayerTurn;

        public int TurnCount { get; private set; }
        public TurnState CurrentState => currentTurnState;

        public void BeginPlayerTurn()
        {
            currentTurnState = TurnState.PlayerTurn;
            Events.RaiseTurnChanged(TurnState.PlayerTurn);
        }

        public void BeginEnemyTurn()
        {
            currentTurnState = TurnState.EnemyTurn;
            Events.RaiseTurnChanged(TurnState.EnemyTurn);
        }

        public void EndPlayerTurn()
        {
            if (currentTurnState != TurnState.PlayerTurn)
            {
                return;
            }

            TurnCount++;
            BeginEnemyTurn();
        }

        public void EndEnemyTurn()
        {
            if (currentTurnState != TurnState.EnemyTurn)
            {
                return;
            }

            currentTurnState = TurnState.PlayerTurn;
            Events.RaiseTurnChanged(TurnState.PlayerTurn);
        }

        public bool IsPlayerTurn()
        {
            return currentTurnState == TurnState.PlayerTurn;
        }

        public bool IsEnemyTurn()
        {
            return currentTurnState == TurnState.EnemyTurn;
        }
    }
}

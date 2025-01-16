

namespace HHSGame.Core.Combat
{
    public class TurnManager
    {

        private TurnState currentTurnState = TurnState.PlayerTurn;

        public TurnManager()
        {

            EventSystem.OnTurnChanged += HandleTurnChanged;
        }

        private void HandleTurnChanged(object? sender, TurnEventArgs e)
        {
            currentTurnState = e.State;
        }

        public void EndPlayerTurn()
        {
            if (currentTurnState == TurnState.PlayerTurn)
            {
                EventSystem.RaiseTurnChanged(TurnState.EnemyTurn);
            }
        }

        public void EndEnemyTurn()
        {
            if (currentTurnState == TurnState.EnemyTurn)
            {
                EventSystem.RaiseTurnChanged(TurnState.PlayerTurn);
            }
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
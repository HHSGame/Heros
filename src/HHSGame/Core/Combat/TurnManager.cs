

namespace HHSGame.Core.Combat {
    public class TurnManager {

        private TurnState _currentTurnState = TurnState.PlayerTurn;

        public TurnManager() {
            
            EventSystem.OnTurnChanged += HandleTurnChanged;
        }

        private void HandleTurnChanged(object? sender, TurnEvent e)
        {
            _currentTurnState = e.State;
        }

        public void EndPlayerTurn() {
            if (_currentTurnState == TurnState.PlayerTurn) {
                EventSystem.RaiseEvent("Ending player turn...");
                EventSystem.RaiseTurnChanged(TurnState.EnemyTurn);
            }
        }

        public void EndEnemyTurn() {
            if (_currentTurnState == TurnState.EnemyTurn) {
                EventSystem.RaiseEvent("Ending enemy turn...");
                EventSystem.RaiseTurnChanged(TurnState.PlayerTurn);
            }
        }

        public bool IsPlayerTurn() {
            return _currentTurnState == TurnState.PlayerTurn;
        }

        public bool IsEnemyTurn() {
            return _currentTurnState == TurnState.EnemyTurn;
        }
    }
}
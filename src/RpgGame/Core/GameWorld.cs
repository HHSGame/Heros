using RpgGame.UI;

namespace RpgGame.Core
{
    public class GameWorld : IDrawable
    {
        public static int MapWidth { get; } = 500;
        public static int MapHeight { get; } = 500;
        
        private TurnState _currentTurn = TurnState.PlayerTurn;
        
        public TurnState CurrentTurn => _currentTurn;

        GameContext _gameContext;

        public GameContext Context => _gameContext;

        public GameWorld(GameContext context)
        {
            _gameContext = context;

            EventSystem.OnTurnChanged += HandleTurnChange;
        }

        private void HandleTurnChange(object? sender, TurnEvent e)
        {
            _currentTurn = e.State;
        }

        public Player NewPlayer() {
            // create a player at the center of the map but avoid any obstacles
            int x = MapWidth / 2, y = MapHeight / 2;
            int round = 0;
            while (!Context.MapState.IsWalkable(x, y))
            {
                x = Context.Random.Next(x - round, x + round);
                y = Context.Random.Next(y - round, y + round);
                round ++;
            }
            return new Player(x, y, this, Context);
        }

        public void Update(Player player)
        {
            // Update all enemies
            Context.EnemyManager.UpdateEnemies(player, _currentTurn == TurnState.EnemyTurn);
            EndEnemyTurn();
        }

        private void EndEnemyTurn()
        {
            if (_currentTurn == TurnState.EnemyTurn) {
                EventSystem.RaiseTurnChanged(TurnState.PlayerTurn);
            }
        }
        
        public void Draw(IDrawingContext ctx)
        {
            // Draw map tiles within viewport
            Context.MapState.Draw(ctx);
            // Draw enemies within viewport
            Context.EnemyManager.Draw(ctx);
            // Draw loot within viewport
            Context.ItemManager.Draw(ctx);
        }
    }
}

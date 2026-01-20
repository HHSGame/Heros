using HHSGame.Core.Combat;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using Microsoft.Extensions.Logging;

namespace HHSGame.Core
{
    public partial class Game(ILogger<Game> logger, GameContext context, GameWorld world)
    {
        public Player? Player { get; private set; }
        private bool isRunning;

        public GameContext Context => context;

        public void Start()
        {
            Classes.ClassConfig classConfig = context.Parameters.PlayerClass ?? Classes.Classes.Warrior;
            Player = world.NewPlayer(classConfig.ToClass());
            LogStartup(logger, "Intializing Context");
            context.InitializeContext(Player);
            isRunning = true;
            context.StateMachine.TryChangeState(GameStateType.Exploration);
            Player.ResetTurn();
            context.TurnManager.BeginPlayerTurn();

            LogStartup(logger, "Player setup");
            Player.AddItem(new HealthPotion(10));

            Events.OnGameMessageEvent += (sender, e) =>
            {
                LogMessage(logger, e.Message);
            };

            RenderFrame();
        }

        [LoggerMessage(LogLevel.Information, "{message}")]
        public static partial void LogMessage(ILogger logger, string message);
        [LoggerMessage(LogLevel.Information, "Game Starting: {message}")]
        public static partial void LogStartup(ILogger logger, string message);

        public bool PerformPlayerAction(Action action, int apCost, bool endTurn, params GameStateType[] allowedStates)
        {
            if (!isRunning || Player == null)
            {
                return false;
            }

            if (!IsStateAllowed(allowedStates))
            {
                return false;
            }

            if (!context.TurnManager.IsPlayerTurn())
            {
                return false;
            }

            if (!Player.Stats.TrySpendAp(apCost))
            {
                return false;
            }

            action();

            RenderFrame();
            if (endTurn || Player.Stats.CurrentAp <= 0)
            {
                EndPlayerTurn();
            }
            return true;
        }

        public bool TryMovePlayer(Move move, params GameStateType[] allowedStates)
        {
            if (!isRunning || Player == null)
            {
                return false;
            }

            if (!IsStateAllowed(allowedStates))
            {
                return false;
            }

            Coordinate target = Player.Position.Move(move.ToVec());
            Enemy? enemy = context.EnemyManager.GetEnemyAt(target.X, target.Y);
            if (enemy != null)
            {
                return PerformPlayerAction(
                    () => Player.Attack(enemy),
                    Player.EquippedWeapon.ApCost,
                    false,
                    allowedStates);
            }

            return PerformPlayerAction(() => Player.Move(move), ActionCosts.Movement, false, allowedStates);
        }

        public void Stop()
        {
            isRunning = false;
            context.StateMachine.TryChangeState(GameStateType.GameOver);
        }

        private void EndPlayerTurn()
        {
            if (Player == null)
            {
                return;
            }

            Player.EndTurn();
            context.TurnManager.EndPlayerTurn();
            world.Update(Player);
            context.TurnManager.EndEnemyTurn();
            Player.ResetTurn();
            context.TurnManager.BeginPlayerTurn();
            RenderFrame();
        }

        private void RenderFrame()
        {
            if (world == null || Player == null)
            {
                return;
            }

            UI.IDrawingContext drawingContext = context.DrawingContext;

            drawingContext.Viewport.UpdateViewport(Player.X, Player.Y, context.MapState.Width, context.MapState.Height);

            world.Draw(drawingContext);
            (Player as IGameActor).Draw(drawingContext);

            drawingContext.Render();
        }

        private bool IsStateAllowed(GameStateType[] allowedStates)
        {
            if (allowedStates.Length == 0)
            {
                return true;
            }

            GameStateType currentState = context.StateMachine.CurrentState;
            foreach (GameStateType state in allowedStates)
            {
                if (state == currentState)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

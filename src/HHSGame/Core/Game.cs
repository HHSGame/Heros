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
            Player = world.NewPlayer(Classes.Classes.Warrior.ToClass());
            LogStartup(logger, "Intializing Context");
            context.InitializeContext(Player);
            isRunning = true;
            context.StateMachine.TryChangeState(GameStateType.Exploration);

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

        public bool PerformPlayerAction(Action action, bool consumesTurn = true, params GameStateType[] allowedStates)
        {
            if (!isRunning || Player == null)
            {
                return false;
            }

            if (!IsStateAllowed(allowedStates))
            {
                return false;
            }

            if (consumesTurn && !context.TurnManager.IsPlayerTurn())
            {
                return false;
            }

            action();

            if (consumesTurn)
            {
                context.TurnManager.EndPlayerTurn();
                world.Update(Player);
            }

            RenderFrame();
            return true;
        }

        public void Stop()
        {
            isRunning = false;
            context.StateMachine.TryChangeState(GameStateType.GameOver);
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

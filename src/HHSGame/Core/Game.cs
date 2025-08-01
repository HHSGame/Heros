using HHSGame.Core.Items;
using Microsoft.Extensions.Logging;
using Terminal.Gui.App;

namespace HHSGame.Core
{
    public partial class Game(ILogger<Game> logger, GameContext context, GameWorld world)
    {
        public Player? Player { get; private set; }
        private bool isRunning;
        private DateTime lastFrameTime;
        private const double TargetFrameTime = 1000.0 / 60.0; // 60 FPS

        public GameContext Context => context;

        public void Start()
        {
            Player = world.NewPlayer(Classes.Classes.Warrior.ToClass());
            LogStartup(logger, "Intializing Context");
            context.InitializeContext(Player);
            isRunning = true;

            LogStartup(logger, "Player setup");
            Player.AddItem(new HealthPotion(10));

            // Start game loop with refresh rate
            Application.AddTimeout(TimeSpan.FromMilliseconds(TargetFrameTime), GameLoop);

            Events.OnGameMessageEvent += (sender, e) =>
            {
                LogMessage(logger, e.Message);
            };
        }

        [LoggerMessage(LogLevel.Information, "{message}")]
        public static partial void LogMessage(ILogger logger, string message);
        [LoggerMessage(LogLevel.Information, "Game Starting: {message}")]
        public static partial void LogStartup(ILogger logger, string message);

        private bool GameLoop()
        {
            if (!isRunning || world == null || Player == null)
            {
                return false;
            }

            UI.IDrawingContext drawingContext = context.DrawingContext;

            // Calculate frame timing
            double deltaTime = CalculateDeltaTime();

            UpdateGame(deltaTime);

            drawingContext.Viewport.UpdateViewport(Player.X, Player.Y, context.MapState.Width, context.MapState.Width);

            world.Draw(drawingContext);
            (Player as IGameActor).Draw(drawingContext);

            // Render to view
            drawingContext.Render();

            // Maintain consistent frame rate
            double frameTime = (DateTime.Now - lastFrameTime).TotalMilliseconds;
            if (frameTime < TargetFrameTime)
            {
                drawingContext.Clear();
                Thread.Sleep((int)(TargetFrameTime - frameTime));
            }

            return isRunning;
        }

        public void Stop()
        {
            isRunning = false;
        }

        private double CalculateDeltaTime()
        {
            DateTime now = DateTime.Now;
            double delta = (now - lastFrameTime).TotalMilliseconds;
            lastFrameTime = now;
            return delta;
        }

        private void UpdateGame(double _)
        {
            if (world == null || Player == null)
            {
                return;
            }

            world.Update(Player);
        }
    }
}

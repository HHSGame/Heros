using HHSGame.Core.Items;
using Microsoft.Extensions.Logging;
using Terminal.Gui.App;

namespace HHSGame.Core
{
    public partial class Game(ILogger<Game> logger, GameContext context, GameWorld world, Player player)
    {
        public Player Player => player;
        private bool isRunning;
        private DateTime lastFrameTime;
        private const double TargetFrameTime = 1000.0 / 60.0; // 60 FPS

        public void Start()
        {
            LogStartup(logger, "Intializing Context");
            context.InitializeContext(player);
            isRunning = true;

            LogStartup(logger, "Player setup");
            player.AddItem(new HealthPotion(10));

            // Start game loop with refresh rate
            Application.AddTimeout(TimeSpan.FromMilliseconds(TargetFrameTime), GameLoop);
        }

        [LoggerMessage(LogLevel.Information, "Game Starting: {message}")]
        public static partial void LogStartup(ILogger logger, string message);

        private bool GameLoop()
        {
            if (!isRunning || world == null || player == null)
            {
                return false;
            }

            UI.IDrawingContext drawingContext = context.DrawingContext;

            // Calculate frame timing
            double deltaTime = CalculateDeltaTime();

            UpdateGame(deltaTime);

            drawingContext.Viewport.UpdateViewport(player.X, player.Y, GameWorld.MapWidth, GameWorld.MapHeight);

            world.Draw(drawingContext);
            (player as IGameActor).Draw(drawingContext);

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
            if (world == null || player == null)
            {
                return;
            }

            world.Update(player);
        }
    }
}

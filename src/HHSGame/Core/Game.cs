using Terminal.Gui;
using HHSGame.UI;

namespace HHSGame.Core
{
    using Items;

    public class Game(GameContext context, GameWorld world, Player player)
    {
        public Player Player => player;
        private bool isRunning;
        private DateTime lastFrameTime;
        private const double TargetFrameTime = 1000.0 / 60.0; // 60 FPS

        public void Start()
        {
            context.InitializeContext();
            isRunning = true;

            player.AddItem(new HealthPotion(10));

            // Start game loop with refresh rate
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(TargetFrameTime), GameLoop);
        }

        private bool GameLoop(MainLoop arg)
        {
            if (!isRunning || world == null || player == null)
                return false;
            var drawingContext = context.DrawingContext;

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
            var now = DateTime.Now;
            var delta = (now - lastFrameTime).TotalMilliseconds;
            lastFrameTime = now;
            return delta;
        }

        private void UpdateGame(double deltaTime)
        {
            if (world == null || player == null)
                return;

            world.Update(player);
        }
    }
}

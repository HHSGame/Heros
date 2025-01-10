using Terminal.Gui;
using HHSGame.UI;

namespace HHSGame.Core
{
    public class Game(GameContext _context) : IDisposable
    {
        public Player? Player => _player;

        public GameWorld? World => _world;
        private GameWorld? _world;
        private Player? _player;
        private bool _isRunning = false;
        private DateTime _lastFrameTime;
        private const double TargetFrameTime = 1000.0 / 60.0; // 60 FPS
        public GameContext Context => _context;

        public void Start()
        {
            _isRunning = true;
            StartNewGame();
        }

        private void StartNewGame()
        {
            // Initialize game world and player
            _world = new GameWorld(_context);
            _player = _world.NewPlayer();
            _player.AddItem(new HealthPotion(10));

            // Start game loop with refresh rate
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(TargetFrameTime), GameLoop);
        }
        
        private bool GameLoop(MainLoop arg)
        {
            var drawingContext = _context.DrawingContext;
            if (!_isRunning || _world == null || _player == null)
                return false;

            // Calculate frame timing
            double deltaTime = CalculateDeltaTime();
            
            UpdateGame(deltaTime);
            
            drawingContext.Viewport.UpdateViewport(_player.X, _player.Y, GameWorld.MapWidth, GameWorld.MapHeight);

            _world.Draw(drawingContext);
            _player.Draw(drawingContext);
            
            // Render to view
            drawingContext.Render();

            // Maintain consistent frame rate
            double frameTime = (DateTime.Now - _lastFrameTime).TotalMilliseconds;
            if (frameTime < TargetFrameTime)
            {
                drawingContext.Clear();
                Thread.Sleep((int)(TargetFrameTime - frameTime));
            }
            
            return _isRunning;
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public void Dispose()
        {
            Stop();
        }

        private double CalculateDeltaTime()
        {
            var now = DateTime.Now;
            var delta = (now - _lastFrameTime).TotalMilliseconds;
            _lastFrameTime = now;
            return delta;
        }

        private void UpdateGame(double deltaTime)
        {
            if (_world == null || _player == null)
                return;

            _world.Update(_player);
        }
    }
}

using Terminal.Gui;
using RpgGame.UI;

namespace RpgGame.Core
{
    public class Game(IDrawingContext _drawingCtx) : IDisposable
    {
        public Player? Player => _player;

        private GameWorld? _world;
        private Player? _player;
        private bool _isRunning = false;
        private DateTime _lastFrameTime;
        private const double TargetFrameTime = 1000.0 / 60.0; // 60 FPS

        public void Start(MapStyle mapStyle = MapStyle.Cave)
        {
            _isRunning = true;
            StartNewGame(mapStyle);
        }

        private void StartNewGame(MapStyle mapStyle)
        {
            // Initialize game world and player
            _world = new GameWorld(mapStyle);
            _player = _world.NewPlayer();

            // Start game loop with refresh rate
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(TargetFrameTime), GameLoop);
        }
        
        private bool GameLoop(MainLoop arg)
        {
            if (!_isRunning || _world == null || _player == null)
                return false;

            // Calculate frame timing
            double deltaTime = CalculateDeltaTime();
            
            UpdateGame(deltaTime);
            
            _drawingCtx.Viewport.UpdateViewport(_player.X, _player.Y, GameWorld.MapWidth, GameWorld.MapHeight);
            _world.Draw(_drawingCtx);
            _player.Draw(_drawingCtx);
            
            // Render to view
            _drawingCtx.Render();

            // Maintain consistent frame rate
            double frameTime = (DateTime.Now - _lastFrameTime).TotalMilliseconds;
            if (frameTime < TargetFrameTime)
            {
                _drawingCtx.Clear();
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

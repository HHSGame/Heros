using Terminal.Gui;
using System.Collections.Generic;
using System;
using RpgGame.UI;

namespace RpgGame.Core
{
    public class Game(IDrawingContext context) : IDisposable
    {
        private GameWorld? _world;
        private Player? _player;
        private bool _isRunning = false;
        private DateTime _lastFrameTime;
        private const double TargetFrameTime = 1000.0 / 60.0; // 60 FPS

        private readonly IDrawingContext _drawingCtx = context;

        public void Start()
        {
            _isRunning = true;
            StartNewGame();
        }

        private void StartNewGame()
        {
            // Initialize game world and player
            _world = new GameWorld();
            _player = new Player(40, 12, _world); // Start player in center
            
            // Start game loop with faster refresh rate
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(10), GameLoop);
        }

        private bool GameLoop(MainLoop arg)
        {
            if (!_isRunning || _world == null || _player == null)
                return false;

            // Calculate frame timing
            double deltaTime = CalculateDeltaTime();
            
            UpdateGame(deltaTime);

            _drawingCtx.Clear();
            
            _world.Draw(_drawingCtx);
            _player.Draw(_drawingCtx);
            
            // Render to view
            _drawingCtx.Render();

            // Maintain consistent frame rate
            double frameTime = (DateTime.Now - _lastFrameTime).TotalMilliseconds;
            if (frameTime < TargetFrameTime)
            {
                Thread.Sleep((int)(TargetFrameTime - frameTime));
            }
            
            return _isRunning;
        }

        public Player? Player => _player;
        public GameWorld? World => _world;

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

            // Update game world
            _world.Update(_player);
        }
    }
}

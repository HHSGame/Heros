using Terminal.Gui;
using System.Collections.Generic;
using System;

namespace RpgGame
{
    public class Game : IDisposable
    {
        private GameWorld? _world;
        private Player? _player;
        private bool _isRunning;
        private DateTime _lastFrameTime;
        private const double TargetFrameTime = 1000.0 / 60.0; // 60 FPS
        private const int MaxEventMessages = 10;
        private readonly List<string> _eventMessages = new();
        
        // Turn-based system
        public enum TurnState
        {
            PlayerTurn,
            EnemyTurn
        }
        public TurnState _currentTurn = TurnState.PlayerTurn;

        private readonly View _view;
        private readonly View _textView;

        private void HandleGameEvent(object? sender, GameEvent e)
        {
            _eventMessages.Insert(0, e.ToString());
            if (_eventMessages.Count > MaxEventMessages)
            {
                _eventMessages.RemoveAt(MaxEventMessages);
            }
        }

        public Game(View view, View textView)
        {
            _isRunning = false;
            _view = view;
            _textView = textView;
            EventSystem.OnGameEvent += HandleGameEvent;
        }
        
        public void Start()
        {
            _isRunning = true;
            StartNewGame();
        }

        private void StartNewGame()
        {
            // Initialize game world and player
            _world = new GameWorld();
            _player = new Player(40, 12, _world, this); // Start player in center
            
            // Start game loop with faster refresh rate
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(100), GameLoop);
        }

        private bool GameLoop(MainLoop arg)
        {
            if (!_isRunning || _world == null || _player == null)
                return false;

            // Calculate frame timing
            double deltaTime = CalculateDeltaTime();
            
            // Process input
            HandleInput();
            
            // Update game state
            UpdateGame(deltaTime);
            // Create and initialize buffer with spaces
            var buffer = new char[GameWorld.MapHeight, GameWorld.MapWidth];
            for (int y = 0; y < GameWorld.MapHeight; y++)
            {
                for (int x = 0; x < GameWorld.MapWidth; x++)
                {
                    buffer[y, x] = ' ';
                }
            }
            
            // Draw to buffer
            _world.Draw(buffer);
            _player.Draw(buffer);
            
            // Convert buffer to text
            var sb = new System.Text.StringBuilder();
            for (int y = 0; y < GameWorld.MapHeight; y++)
            {
                for (int x = 0; x < GameWorld.MapWidth; x++)
                {
                    sb.Append(buffer[y, x]);
                }
                sb.Append('\n');
            }
            
            // Render to view
            _view.Text = sb.ToString();

            // Update display
            var eventText = string.Join("\n", _eventMessages);
            _textView.Text = $"Player is at {_player.X}, {_player.Y}, Health: {_player.Health}.\n{eventText}";

            // Maintain consistent frame rate
            double frameTime = (DateTime.Now - _lastFrameTime).TotalMilliseconds;
            if (frameTime < TargetFrameTime)
            {
                System.Threading.Thread.Sleep((int)(TargetFrameTime - frameTime));
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
            // _player?.Clear(_view);
            // _world?.Clear(_view);
        }

        public void EndPlayerTurn()
        {
            if (_currentTurn != TurnState.PlayerTurn) return;
            
            _currentTurn = TurnState.EnemyTurn;
        }
        public void EndSystemTurn()
        {
            if (_currentTurn != TurnState.EnemyTurn) return;
            
            _currentTurn = TurnState.PlayerTurn;
        }

        private double CalculateDeltaTime()
        {
            var now = DateTime.Now;
            var delta = (now - _lastFrameTime).TotalMilliseconds;
            _lastFrameTime = now;
            return delta;
        }

        private void HandleInput()
        {
            // Input is handled by Program.cs through KeyPress event
        }

        private void UpdateGame(double deltaTime)
        {
            if (_world == null || _player == null)
                return;

            // Update game world with scaled delta time
            _world.Update(_player, _currentTurn);
            EndSystemTurn();
        }
    }
}

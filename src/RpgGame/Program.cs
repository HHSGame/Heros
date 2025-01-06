using System;
using Terminal.Gui;

namespace RpgGame
{
    class Program
    {
        private static Game? currentGame;

        static void Main(string[] args)
        {
            Application.Init();
            
            // Create main window
            var top = Application.Top;

            // Initialize and start game

            // Create main window with full dimensions
            var viewWin = new Window("RPG Game") {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 10,
                ColorScheme = new ColorScheme {
                    Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                    Focus = Application.Driver.MakeAttribute(Color.White, Color.Black)
                }
            };

            var view = new View {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            var consoleWin = new Window("Messages") {
                X = 0,
                Y = Pos.Bottom(viewWin),
                Width = Dim.Fill(),
                Height = 10,
                ColorScheme = new ColorScheme {
                    Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
                    Focus = Application.Driver.MakeAttribute(Color.White, Color.Black)
                }
            };

            // Text view fixed at bottom with 10 line height
            View tv = new View {
                X = 0,
                Y = 0,
                Height = 10,
                Width = Dim.Fill()
            };

            viewWin.Add(view);
            // Add views to container
            consoleWin.Add(tv);
            top.Add(viewWin, consoleWin);
            tv.Text += $"Window dimensions: {view.Bounds}\n";
            tv.Text += $"View dimensions: {tv.Bounds}\n";
            
            StartNewGame(view, tv);
            
            view.Visible = true;
            tv.Visible = true;
            
            // Handle key events
            Application.RootKeyEvent += (args) => {
                // Process movement keys
                switch (args.Key)
                    {
                        case Key.CursorUp:
                            currentGame?.Player?.Move(0, -1);
                            break;
                        case Key.CursorDown:
                            currentGame?.Player?.Move(0, 1);
                            break;
                        case Key.CursorLeft:
                            currentGame?.Player?.Move(-1, 0);
                            break;
                        case Key.CursorRight:
                            currentGame?.Player?.Move(1, 0);
                            break;
                        case Key.Q:
                            currentGame?.Stop();
                            Application.RequestStop();
                            Application.Shutdown();
                            return true;
                        default:
                            // Let other keys propagate
                            return false;
                    }
                    // Mark event as handled
                    return true;
            };

            Application.Run();
            Application.Shutdown();
        }

        private static void StartNewGame(View view, View textView)
        {
            try
            {
                var game = new Game(view, textView);
                currentGame = game;
                game.Start();
            }
            catch (Exception ex)
            {
                textView.Text += $"Error starting new game: {ex.Message}\n";
            }
        }
    }
}

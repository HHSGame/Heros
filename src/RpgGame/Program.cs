using Terminal.Gui;
using RpgGame.UI;

namespace RpgGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();

            new GameUI().Start();
        }
    }
}

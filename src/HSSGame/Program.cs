using Terminal.Gui;
using HHSGame.UI;

namespace HHSGame
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

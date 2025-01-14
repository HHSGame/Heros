
using Terminal.Gui;

namespace HHSGame.UI
{
    public class GUISettings
    {
        public static int SidebarWidth => 20;
        public static int MessageWindowHeight => 10;

        public static string MapWindowTitle => "Main Map";
        public static string MessageWindowTitle => "Messages";
        public static string InventoryWindowTitle => "Inventory";
        public static string SurroundingsWindowTitle => "Surroundings";
        public static string UtilityWindowTitle => "Utilities";

        public static ColorScheme CommonWindowColorScheme => new ColorScheme
        {
            Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
            Focus = Application.Driver.MakeAttribute(Color.White, Color.Black)
        };

    }
}

using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;

namespace HHSGame.UI
{
    public class GUISettings
    {
        public static int SidebarWidth => 20;
        public static int MessageWindowHeight => 10;
        public static int StatusBarHeight => 1;
        public static int ActionSequenceWidth => 24;
        public static int ActionStepDelayMs => 200;

        public static string MapWindowTitle => "Main Map";
        public static string MessageWindowTitle => "Messages";
        public static string ActionSequenceWindowTitle => "Action Sequence";
        public static string InventoryWindowTitle => "Inventory";
        public static string SurroundingsWindowTitle => "Surroundings";
        public static string UtilityWindowTitle => "Utilities";

        public static Scheme CommonWindowColorScheme => SchemeManager.GetScheme(Schemes.Toplevel);

        public static char PathPreviewGlyph => '*';
        public static char TargetPreviewGlyph => 'X';
        public static char PlannedDestinationGlyph => 'o';
        public static char RangePreviewGlyph => '+';

    }
}

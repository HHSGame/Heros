using HHSGame.Core.Rendering;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;

namespace HHSGame
{
    /// <summary>
    /// Terminal.Gui 特定的 GUISettings 扩展。
    /// </summary>
    public static class GUISettingsExtensions
    {
        public static Scheme CommonWindowColorScheme => SchemeManager.GetScheme(Schemes.Toplevel);
    }
}

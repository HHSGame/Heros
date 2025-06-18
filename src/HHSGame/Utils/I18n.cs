using System.Globalization;
using System.Resources;

namespace HHSGame.Utils
{
    public static class I18n
    {
        private static readonly ResourceManager resourceManager = new("HHSGame.Resources.Localization", typeof(I18n).Assembly);

        public static string GetString(string key, params object[] args)
        {
            string? template = resourceManager.GetString(key, CultureInfo.CurrentCulture);
            if (string.IsNullOrEmpty(template))
            {
                return key; // Fallback to key if not found
            }
            return string.Format(CultureInfo.CurrentCulture, template, args);
        }
    }
}

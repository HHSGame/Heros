using System.Globalization;
using System.Resources;

namespace HHSGame.Utils
{
    public static class I18n
    {
        private static readonly ResourceManager resourceManager = new("HHSGame.Core.Resources.Localization", typeof(I18n).Assembly);

        public static string T(string key, params object[] args)
        {
            string? template = resourceManager.GetString(key, CultureInfo.GetCultureInfoByIetfLanguageTag("zh-CN"));
            if (string.IsNullOrEmpty(template))
            {
                return key; // Fallback to key if not found
            }
            return string.Format(CultureInfo.CurrentCulture, template, args);
        }
    }
}

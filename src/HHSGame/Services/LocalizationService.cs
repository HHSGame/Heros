using System.Resources;

namespace HHSGame.Services
{
    public static class LocalizationService
    {
        private static readonly ResourceManager _resourceManager = new ResourceManager("HHSGame.Resources.Localization", typeof(LocalizationService).Assembly);

        public static string GetString(string key, params object[] args)
        {
            var template = _resourceManager.GetString(key);
            if (string.IsNullOrEmpty(template))
            {
                return key; // Fallback to key if not found
            }
            return string.Format(template, args);
        }
    }
}

#if GOOGLE_PLAY
using infrastructure.services.platform.core.language;
using UnityEngine;

namespace infrastructure.services.platform.google
{
    /// <summary>
    /// Язык из системных настроек устройства.
    /// </summary>
    public class GooglePlayLanguageProvider : ILanguageService
    {
        public string CurrentLanguage =>
            Application.systemLanguage == SystemLanguage.Russian ? "ru" : "en";
    }
}
#endif

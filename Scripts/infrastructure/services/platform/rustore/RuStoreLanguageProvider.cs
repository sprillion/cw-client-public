#if RU_STORE
using infrastructure.services.platform.core.language;
using UnityEngine;

namespace infrastructure.services.platform.rustore
{
    /// <summary>
    /// Язык из системных настроек устройства.
    /// </summary>
    public class RuStoreLanguageProvider : ILanguageService
    {
        public string CurrentLanguage =>
            Application.systemLanguage == SystemLanguage.Russian ? "ru" : "en";
    }
}
#endif

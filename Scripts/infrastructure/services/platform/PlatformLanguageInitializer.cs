using System.Collections.Generic;
using I2.Loc;
using infrastructure.services.platform.core.language;
using UnityEngine;

namespace infrastructure.services.platform
{
    public class PlatformLanguageInitializer
    {
        private const string I2LocPlayerPrefsKey = "I2 Language";

        private static readonly Dictionary<string, string> IsoToI2Loc = new()
        {
            { "ru", "Russian" },
            { "en", "English" },
            { "tr", "Turkish" },
            { "de", "German" },
            { "fr", "French" },
            { "uk", "Ukrainian" },
        };

        public PlatformLanguageInitializer(ILanguageService language)
        {
            if (PlayerPrefs.HasKey(I2LocPlayerPrefsKey))
                return;

            var iso = language.CurrentLanguage;

            if (!IsoToI2Loc.TryGetValue(iso, out var i2LocName))
                i2LocName = "English";

            if (!LocalizationManager.HasLanguage(i2LocName))
                i2LocName = "English";

            LocalizationManager.CurrentLanguage = i2LocName;
            Debug.Log($"[PlatformLanguage] Applied platform language: {iso} → {i2LocName}");
        }
    }
}

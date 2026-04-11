#if VK_PLAY
using infrastructure.services.platform.core.language;
using UnityEngine;

namespace infrastructure.services.platform.vk
{
    // Язык парсится VKAuthProvider из URL-параметров VK и сохраняется в PlayerPrefs.
    // Здесь просто читаем сохранённое значение.
    public class VKLanguageProvider : ILanguageService
    {
        public string CurrentLanguage => PlayerPrefs.GetString("vk_language", "ru");
    }
}
#endif

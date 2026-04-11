#if YANDEX_GAMES
using YandexGames;
using infrastructure.services.platform.core.language;

namespace infrastructure.services.platform.yandex
{
    public class YandexLanguageProvider : ILanguageService
    {
        public string CurrentLanguage => YandexGamesSDK.Environment?.i18n.lang ?? "ru";
    }
}
#endif

using infrastructure.services.platform.core.language;

namespace infrastructure.services.platform.stubs
{
    public class EditorLanguageProvider : ILanguageService
    {
        public string CurrentLanguage => "ru";
    }
}

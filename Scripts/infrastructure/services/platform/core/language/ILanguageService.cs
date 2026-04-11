namespace infrastructure.services.platform.core.language
{
    public interface ILanguageService
    {
        // ISO 639-1: "ru", "en", etc.
        string CurrentLanguage { get; }
    }
}

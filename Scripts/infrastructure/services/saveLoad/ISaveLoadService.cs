namespace infrastructure.services.saveLoad
{
    public interface ISaveLoadService
    {
        bool HasToken();
        void SetToken(string token);
        string GetToken();
        void SetJson(string key, string json);
        string GetJson(string key);
        bool HasJson(string key);
    }
}
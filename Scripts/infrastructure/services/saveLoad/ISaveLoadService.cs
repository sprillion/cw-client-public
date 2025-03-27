namespace infrastructure.services.saveLoad
{
    public interface ISaveLoadService
    {
        bool HasToken();
        void SetToken(string token);
        string GetToken();
    }
}
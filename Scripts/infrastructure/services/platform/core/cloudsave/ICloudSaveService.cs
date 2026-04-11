using Cysharp.Threading.Tasks;

namespace infrastructure.services.platform.core.cloudsave
{
    public interface ICloudSaveService
    {
        UniTask Save(string key, string json);
        UniTask<string> Load(string key); // returns null if key not found
    }
}

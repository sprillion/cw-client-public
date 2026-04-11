using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.cloudsave;
using UnityEngine;

namespace infrastructure.services.platform.stubs
{
    public class EditorCloudSaveService : ICloudSaveService
    {
        public UniTask Save(string key, string json)
        {
            PlayerPrefs.SetString(CloudKey(key), json);
            PlayerPrefs.Save();
            Debug.Log($"[CloudSave] Saved key={key}");
            return UniTask.CompletedTask;
        }

        public UniTask<string> Load(string key)
        {
            var cloudKey = CloudKey(key);
            var result = PlayerPrefs.HasKey(cloudKey) ? PlayerPrefs.GetString(cloudKey) : null;
            return UniTask.FromResult(result);
        }

        private static string CloudKey(string key) => $"cloud_{key}";
    }
}

#if YANDEX_GAMES
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.cloudsave;
using UnityEngine;
using YandexGames;

namespace infrastructure.services.platform.yandex
{
    public class YandexCloudSaveProvider : ICloudSaveService
    {
        private Dictionary<string, string> _cache;

        public async UniTask<string> Load(string key)
        {
            if (_cache == null)
                await FetchAll();

            return _cache.TryGetValue(key, out var value) ? value : null;
        }

        public async UniTask Save(string key, string json)
        {
            if (_cache == null)
                await FetchAll();

            _cache[key] = json;
            await PushAll();
        }

        private async UniTask FetchAll()
        {
            await UniTask.WaitUntil(() => YandexGamesSDK.IsInitialized);

            if (!PlayerAccount.IsAuthorized)
            {
                _cache = new Dictionary<string, string>();
                return;
            }

            var tcs = new UniTaskCompletionSource<string>();
            PlayerAccount.GetCloudSaveData(
                onSuccessCallback: data => tcs.TrySetResult(data),
                onErrorCallback: err =>
                {
                    Debug.LogWarning($"[CloudSave] Load failed: {err}");
                    tcs.TrySetResult("{}");
                });

            var rawJson = await tcs.Task;
            _cache = ParseJson(rawJson);
        }

        private async UniTask PushAll()
        {
            if (!PlayerAccount.IsAuthorized)
            {
                Debug.LogWarning("[CloudSave] Cannot save — player not authorized.");
                return;
            }

            var rawJson = SerializeJson(_cache);
            var tcs = new UniTaskCompletionSource<bool>();
            PlayerAccount.SetCloudSaveData(rawJson,
                onSuccessCallback: () => tcs.TrySetResult(true),
                onErrorCallback: err =>
                {
                    Debug.LogWarning($"[CloudSave] Save failed: {err}");
                    tcs.TrySetResult(false);
                });

            await tcs.Task;
        }

        private static Dictionary<string, string> ParseJson(string rawJson)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(rawJson) || rawJson == "{}")
                return result;

            var state = JsonUtility.FromJson<SaveState>(rawJson);
            if (state?.entries == null)
                return result;

            foreach (var entry in state.entries)
                result[entry.key] = entry.value;

            return result;
        }

        private static string SerializeJson(Dictionary<string, string> data)
        {
            var state = new SaveState();
            foreach (var kvp in data)
                state.entries.Add(new SaveEntry { key = kvp.Key, value = kvp.Value });
            return JsonUtility.ToJson(state);
        }

        [Serializable]
        private class SaveState
        {
            public List<SaveEntry> entries = new List<SaveEntry>();
        }

        [Serializable]
        private class SaveEntry
        {
            public string key;
            public string value;
        }
    }
}
#endif

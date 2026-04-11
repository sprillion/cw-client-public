#if VK_PLAY
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.auth;
using UnityEngine;

namespace infrastructure.services.platform.vk
{
    // MonoBehaviour — нужен для приёма SendMessage-колбэков из JS.
    public class VKAuthProvider : MonoBehaviour, IPlatformAuthProvider
    {
        public bool IsAuthenticated { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void VKInit(string gameObject);

        [DllImport("__Internal")]
        private static extern void VKGetParams(string gameObject);
#endif

        private UniTaskCompletionSource<bool> _initTcs;
        private UniTaskCompletionSource<bool> _paramsDoneTcs;
        private readonly List<string> _rawParams = new List<string>();

        public async UniTask<string> Authenticate()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // 1. Инициализируем VK Bridge
            Debug.Log("[VKAuth] Calling VKInit...");
            _initTcs = new UniTaskCompletionSource<bool>();
            VKInit(gameObject.name);
            var initResult = await _initTcs.Task;
            Debug.Log($"[VKAuth] VKInit result: {initResult}");
            if (!initResult) return null;

            // 2. Собираем все URL-параметры запуска VK Games
            Debug.Log("[VKAuth] Calling VKGetParams...");
            _rawParams.Clear();
            _paramsDoneTcs = new UniTaskCompletionSource<bool>();
            VKGetParams(gameObject.name);
            await _paramsDoneTcs.Task;

            Debug.Log($"[VKAuth] Got {_rawParams.Count} params.");
            if (_rawParams.Count == 0) return null;

            // 3. Сохраняем язык для VKLanguageProvider
            foreach (var param in _rawParams)
            {
                var sep = param.IndexOf('=');
                if (sep < 0) continue;
                if (param.Substring(0, sep) == "vk_language")
                    PlayerPrefs.SetString("vk_language", param.Substring(sep + 1));
            }

            IsAuthenticated = true;

            // 4. Возвращаем все параметры как query string для сервера
            //    Сервер верифицирует подпись (sign) через HMAC-SHA256
            var queryString = string.Join("&", _rawParams);
            Debug.Log($"[VKAuth] Authenticated. Returning query string ({queryString.Length} chars).");
            return queryString;
#else
            IsAuthenticated = false;
            return null;
#endif
        }

        // Колбэки из JS через UnitySendMessage:

        private void OnVKInitSuccess() => _initTcs?.TrySetResult(true);
        private void OnVKInitFailed() => _initTcs?.TrySetResult(false);

        private void OnVKParam(string param)
        {
            if (!string.IsNullOrEmpty(param))
                _rawParams.Add(param);
        }

        private void OnVKParamsDone() => _paramsDoneTcs?.TrySetResult(true);
    }
}
#endif

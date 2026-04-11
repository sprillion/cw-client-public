using Cysharp.Threading.Tasks;
using UnityEngine;

namespace infrastructure.services.platform.core.auth
{
    /// <summary>
    /// Гостевой вход — не требует SDK, возвращает null.
    /// IAuthorization.Authorize() сам возьмёт сохранённый токен или запросит регистрацию.
    /// </summary>
    public class GuestAuthProvider : IPlatformAuthProvider
    {
        public bool IsAuthenticated { get; private set; }

        public async UniTask<string> Authenticate()
        {
            Debug.Log("[GuestAuth] Guest login — no platform token");
            IsAuthenticated = true;
            await UniTask.CompletedTask;
            return null;
        }
    }
}

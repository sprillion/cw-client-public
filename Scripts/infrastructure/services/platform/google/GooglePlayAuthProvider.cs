#if GOOGLE_PLAY
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.auth;

namespace infrastructure.services.platform.google
{
    /// <summary>
    /// Авторизация на Google Play не требует Google Play Games Plugin.
    /// Возвращает null → PlatformAuthFlow вызывает IAuthorization.Authorize() →
    /// AuthView показывает кнопки по PlatformSettings.allow*.
    /// </summary>
    public class GooglePlayAuthProvider : IPlatformAuthProvider
    {
        public bool IsAuthenticated { get; private set; }

        public async UniTask<string> Authenticate()
        {
            IsAuthenticated = true;
            await UniTask.CompletedTask;
            return null;
        }
    }
}
#endif

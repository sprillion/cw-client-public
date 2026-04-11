#if RU_STORE
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.auth;

namespace infrastructure.services.platform.rustore
{
    /// <summary>
    /// На RuStore нет собственной auth-системы. Возвращает null →
    /// PlatformAuthFlow вызывает IAuthorization.Authorize() →
    /// AuthView показывает все доступные кнопки (по PlatformSettings.allow*).
    /// </summary>
    public class RuStoreAuthProvider : IPlatformAuthProvider
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

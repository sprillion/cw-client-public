#if YANDEX_GAMES
using YandexGames;
using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.auth;

namespace infrastructure.services.platform.yandex
{
    public class YandexAuthProvider : IPlatformAuthProvider
    {
        public bool IsAuthenticated { get; private set; }

        public async UniTask<string> Authenticate()
        {
            await UniTask.WaitUntil(() => YandexGamesSDK.IsInitialized);

            if (!PlayerAccount.IsAuthorized)
            {
                var authorized = await AuthorizeAsync();
                if (!authorized) return null;
            }

            var profile = await GetProfileAsync();
            if (profile == null) return null;

            IsAuthenticated = true;
            return profile.uniqueID;
        }

        private UniTask<bool> AuthorizeAsync()
        {
            var tcs = new UniTaskCompletionSource<bool>();
            PlayerAccount.Authorize(
                onSuccessCallback: () => tcs.TrySetResult(true),
                onErrorCallback: _ => tcs.TrySetResult(false));
            return tcs.Task;
        }

        private UniTask<PlayerAccountProfileDataResponse> GetProfileAsync()
        {
            var tcs = new UniTaskCompletionSource<PlayerAccountProfileDataResponse>();
            PlayerAccount.GetProfileData(
                onSuccessCallback: r => tcs.TrySetResult(r),
                onErrorCallback: _ => tcs.TrySetResult(null));
            return tcs.Task;
        }
    }
}
#endif

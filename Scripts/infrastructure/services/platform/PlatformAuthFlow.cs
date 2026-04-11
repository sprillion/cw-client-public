using Cysharp.Threading.Tasks;
using infrastructure.services.auth;
using infrastructure.services.platform.core;

namespace infrastructure.services.platform
{
    public class PlatformAuthFlow
    {
        private readonly IPlatformService _platformService;
        private readonly IAuthorization _authorization;

        public PlatformAuthFlow(IPlatformService platformService, IAuthorization authorization)
        {
            _platformService = platformService;
            _authorization = authorization;
        }

        public async UniTask Run()
        {
            var settings = _platformService.Settings;
            var token = await _platformService.Auth.Authenticate();

            if (token != null && settings.allowYandex)
                _authorization.LoginYandex(token);
            else if (token != null && settings.allowVK && settings.platform == core.Platform.VK)
                _authorization.LoginVKGames(token);
            else if (token != null && settings.allowVK)
                _authorization.LoginVK(token);
            else if (token != null && settings.allowGoogle)
                _authorization.LoginGoogle(token);
            else
                _authorization.Authorize();
        }
    }
}

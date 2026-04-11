using System;
using network;

namespace infrastructure.services.auth
{
    public interface IAuthorization : IReceiver
    {
        event Action OnAuthRequired;
        event Action OnNicknameValid;
        event Action<string> OnNicknameError;
        event Action<LinkedPlatformsData> OnLinkedPlatformsReceived;
        event Action<LinkResultData> OnLinkResult;
        void Authorize();
        void ValidateNickname(string nickname);
        void Registration(string nickname, int skinId);
        void LoginVK(string vkToken);
        void LoginVKGames(string signedParams);
        void LoginYandex(string yandexToken);
        void LoginGoogle(string googleToken);
        void RequestLinkedPlatforms();
        void LinkVK(string token);
        void LinkYandex(string token);
        void LinkGoogle(string token);
    }
}
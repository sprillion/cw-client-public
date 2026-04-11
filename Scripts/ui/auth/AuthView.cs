using infrastructure.services.auth;
using infrastructure.services.platform.core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Popup = ui.popup.Popup;

namespace ui.auth
{
    public class AuthView : Popup
    {
        [SerializeField] private Button _vkButton;
        [SerializeField] private Button _yandexButton;
        [SerializeField] private Button _googleButton;
        [SerializeField] private Button _guestButton;
        
        [SerializeField] private NicknameSetupView _nicknameSetupView;

        private IAuthorization _authorization;


        [Inject]
        public void Construct(IAuthorization authorization, PlatformSettings settings)
        {
            _authorization = authorization;
            _authorization.OnAuthRequired += Show;
            _vkButton.onClick.AddListener(OnVkLogin);
            _yandexButton.onClick.AddListener(OnYandexLogin);
            _googleButton.onClick.AddListener(OnGoogleLogin);
            _guestButton.onClick.AddListener(OnGuestLogin);

            _vkButton.gameObject.SetActive(settings.allowVK);
            _yandexButton.gameObject.SetActive(settings.allowYandex);
            _googleButton.gameObject.SetActive(settings.allowGoogle);
            _guestButton.gameObject.SetActive(settings.allowGuest);
        }

        private void OnVkLogin()
        {
            VKAuthManager.Instance.OnAuthSuccess += OnVkSuccess;
            VKAuthManager.Instance.OnAuthFailed += OnVkFailed;
            VKAuthManager.Instance.OnAuthCanceled += OnVkCanceled;
            VKAuthManager.Instance.Login();
        }

        private void OnVkSuccess(string userId, string token)
        {
            UnsubscribeVK();
            _authorization.LoginVK(token);
            Hide();
        }

        private void OnVkFailed(string error)
        {
            UnsubscribeVK();
            Debug.LogWarning($"[AuthView] VK auth failed: {error}");
        }

        private void OnVkCanceled()
        {
            UnsubscribeVK();
        }

        private void UnsubscribeVK()
        {
            VKAuthManager.Instance.OnAuthSuccess -= OnVkSuccess;
            VKAuthManager.Instance.OnAuthFailed -= OnVkFailed;
            VKAuthManager.Instance.OnAuthCanceled -= OnVkCanceled;
        }

        private void OnYandexLogin()
        {
            YandexAuthManager.Instance.OnAuthSuccess += OnYandexSuccess;
            YandexAuthManager.Instance.OnAuthFailed += OnYandexFailed;
            YandexAuthManager.Instance.OnAuthCanceled += OnYandexCanceled;
            YandexAuthManager.Instance.Login();
        }

        private void OnYandexSuccess(string token)
        {
            UnsubscribeYandex();
            _authorization.LoginYandex(token);
            Hide();
        }

        private void OnYandexFailed(string error)
        {
            UnsubscribeYandex();
            Debug.LogWarning($"[AuthView] Yandex auth failed: {error}");
        }

        private void OnYandexCanceled()
        {
            UnsubscribeYandex();
        }

        private void UnsubscribeYandex()
        {
            YandexAuthManager.Instance.OnAuthSuccess -= OnYandexSuccess;
            YandexAuthManager.Instance.OnAuthFailed -= OnYandexFailed;
            YandexAuthManager.Instance.OnAuthCanceled -= OnYandexCanceled;
        }

        private void OnGoogleLogin()
        {
            GoogleAuthManager.Instance.OnAuthSuccess += OnGoogleSuccess;
            GoogleAuthManager.Instance.OnAuthFailed += OnGoogleFailed;
            GoogleAuthManager.Instance.OnAuthCanceled += OnGoogleCanceled;
            GoogleAuthManager.Instance.Login();
        }

        private void OnGoogleSuccess(string idToken)
        {
            UnsubscribeGoogle();
            _authorization.LoginGoogle(idToken);
            Hide();
        }

        private void OnGoogleFailed(string error)
        {
            UnsubscribeGoogle();
            Debug.LogWarning($"[AuthView] Google auth failed: {error}");
        }

        private void OnGoogleCanceled()
        {
            UnsubscribeGoogle();
        }

        private void UnsubscribeGoogle()
        {
            GoogleAuthManager.Instance.OnAuthSuccess -= OnGoogleSuccess;
            GoogleAuthManager.Instance.OnAuthFailed -= OnGoogleFailed;
            GoogleAuthManager.Instance.OnAuthCanceled -= OnGoogleCanceled;
        }

        private void OnGuestLogin()
        {
            _nicknameSetupView.Show(this);
        }
    }
}

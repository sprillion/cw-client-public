using infrastructure.services.auth;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.settings
{
    public class AccountSettingsView : Popup
    {
        [SerializeField] private Image _vkStatusIcon;
        [SerializeField] private Image _yandexStatusIcon;
        [SerializeField] private Image _googleStatusIcon;

        [SerializeField] private Button _linkVkButton;
        [SerializeField] private Button _linkYandexButton;
        [SerializeField] private Button _linkGoogleButton;

        private IAuthorization _authorization;

        [Inject]
        public void Construct(IAuthorization authorization)
        {
            _authorization = authorization;
        }

        public override void Initialize()
        {
            _authorization.OnLinkedPlatformsReceived += OnLinkedPlatformsReceived;
            _authorization.OnLinkResult += OnLinkResultReceived;

            _linkVkButton.onClick.AddListener(OnLinkVkClicked);
            _linkYandexButton.onClick.AddListener(OnLinkYandexClicked);
            _linkGoogleButton.onClick.AddListener(OnLinkGoogleClicked);
        }

        public override void Show()
        {
            base.Show();
            _authorization.RequestLinkedPlatforms();
        }

        private void OnLinkedPlatformsReceived(LinkedPlatformsData data)
        {
            _vkStatusIcon.gameObject.SetActive(data.HasVK);
            _yandexStatusIcon.gameObject.SetActive(data.HasYandex);
            _googleStatusIcon.gameObject.SetActive(data.HasGoogle);

            _linkVkButton.interactable = !data.HasVK;
            _linkYandexButton.interactable = !data.HasYandex;
            _linkGoogleButton.interactable = !data.HasGoogle;
        }

        private void OnLinkResultReceived(LinkResultData data)
        {
            if (data.Success)
            {
                Debug.Log($"[AccountSettings] Linked {data.Platform} successfully");
                _authorization.RequestLinkedPlatforms();
            }
            else
            {
                Debug.LogWarning($"[AccountSettings] Link {data.Platform} failed: {data.Error}");
            }
        }

        private void OnLinkVkClicked()
        {
            // VK OAuth token should be obtained via platform-specific SDK before calling LinkVK
            Debug.Log("[AccountSettings] Link VK clicked — obtain VK token via SDK");
        }

        private void OnLinkYandexClicked()
        {
            Debug.Log("[AccountSettings] Link Yandex clicked — obtain Yandex token via SDK");
        }

        private void OnLinkGoogleClicked()
        {
            Debug.Log("[AccountSettings] Link Google clicked — obtain Google token via SDK");
        }
    }
}

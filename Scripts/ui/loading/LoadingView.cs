using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using infrastructure.services.loading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.loading
{
    public class LoadingView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _stageText;
        [SerializeField] private LoadingPanel _loadingPanel;
        [SerializeField] private Button _retryButton;

        private static LocalizedString ConnectingToServer = "Game/ConnectingToServer";
        private static LocalizedString VersionCheck = "Game/VersionCheck";
        private static LocalizedString LoadingMap = "Game/LoadingMap";
        private static LocalizedString LoadingMobs = "Game/LoadingMobs";
        private static LocalizedString Authorization = "Game/Authorization";
        private static LocalizedString LoadingPlayerData = "Game/LoadingPlayerData";
        private static LocalizedString CreationOfTheWorld = "Game/CreationOfTheWorld";
        private static LocalizedString Completion = "Game/Completion";
        private static LocalizedString TheServerIsUnavailable = "Game/TheServerIsUnavailable";
        private static LocalizedString TheGameNeedsAnUpdate = "Game/TheGameNeedsAnUpdate";
        private static LocalizedString ConnectionLostReconnecting = "Game/ConnectionLostReconnecting";

        private ILoadingService _loadingService;

        private static readonly Dictionary<LoadingStage, LocalizedString> StageTexts = new()
        {
            { LoadingStage.Connecting, ConnectingToServer },
            { LoadingStage.CheckingVersion, VersionCheck },
            { LoadingStage.LoadingMap, LoadingMap },
            { LoadingStage.LoadingNpc, LoadingMobs },
            { LoadingStage.Authorizing, Authorization },
            { LoadingStage.LoadingPlayerData, LoadingPlayerData },
            { LoadingStage.CreatingWorld, CreationOfTheWorld },
            { LoadingStage.Finalizing, Completion },
            { LoadingStage.ServerUnavailable, TheServerIsUnavailable },
            { LoadingStage.VersionMismatch, TheGameNeedsAnUpdate },
            { LoadingStage.Reconnecting, ConnectionLostReconnecting },
        };

        [Inject]
        public void Construct(ILoadingService loadingService)
        {
            _loadingService = loadingService;
            _loadingService.OnStageChanged += OnStageChanged;
            _loadingService.Loaded += OnLoaded;
            _loadingService.OnServerUnavailable += OnServerUnavailable;

            if (_retryButton != null)
            {
                _retryButton.gameObject.SetActive(false);
                _retryButton.onClick.AddListener(OnRetryClicked);
            }

            gameObject.SetActive(true);
        }

        private void Start()
        {
            _loadingPanel.Show();
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            OnStageChanged(_loadingService.CurrentStage);
        }

        private void OnStageChanged(LoadingStage stage)
        {
            if (stage == LoadingStage.Reconnecting)
            {
                gameObject.SetActive(true);
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                _loadingPanel.Show();
            }

            if (stage is LoadingStage.VersionMismatch)
                _loadingPanel.Hide();

            if (_retryButton != null && stage != LoadingStage.ServerUnavailable)
                _retryButton.gameObject.SetActive(false);

            if (StageTexts.TryGetValue(stage, out var text))
                _stageText.text = text.ToString();
        }

        private void OnServerUnavailable()
        {
            if (_retryButton != null)
                _retryButton.gameObject.SetActive(true);
            _loadingPanel.Hide();
        }

        private void OnRetryClicked()
        {
            if (_retryButton != null)
                _retryButton.gameObject.SetActive(false);
            _loadingService.Retry();
        }

        private void OnLoaded()
        {
            _loadingPanel.Hide();
            _canvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
            });
        }

        private void OnDestroy()
        {
            if (_loadingService == null) return;
            _loadingService.OnStageChanged -= OnStageChanged;
            _loadingService.Loaded -= OnLoaded;
            _loadingService.OnServerUnavailable -= OnServerUnavailable;
        }
    }
}
using System.Text.RegularExpressions;
using infrastructure.services.auth;
using TMPro;
using tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Popup = ui.popup.Popup;

namespace ui.auth
{
    public class NicknameSetupView : Popup
    {
        private const int MinLength = 3;
        private const int MaxLength = 20;

        // Разрешены: латиница, кириллица, цифры, пробел, _ и -
        private static readonly Regex AllowedChars = new Regex(@"^[a-zA-Zа-яА-ЯёЁ0-9 _-]+$");

        [SerializeField] private GameObject _nicknamePanel;
        [SerializeField] private TMP_InputField _nicknameInput;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TMP_Text _errorText;

        [SerializeField] private GameObject _skinPanel;
        [SerializeField] private Button _skin0Button;
        [SerializeField] private Button _skin1Button;
        [SerializeField] private Button _startButton;

        [SerializeField] private Image _skin0Bg;
        [SerializeField] private Image _skin1Bg;

        private IAuthorization _authorization;
        private string _nickname;
        private int _selectedSkinId;

        [Inject]
        public void Construct(IAuthorization authorization)
        {
            _authorization = authorization;
            _authorization.OnNicknameValid += OnNicknameValidated;
            _authorization.OnNicknameError += OnNicknameRejected;

            _nextButton.onClick.AddListener(OnNext);
            _startButton.onClick.AddListener(OnStart);
            _skin0Button.onClick.AddListener(() => SelectSkin(0));
            _skin1Button.onClick.AddListener(() => SelectSkin(1));
        }

        private void OnDestroy()
        {
            _authorization.OnNicknameValid -= OnNicknameValidated;
            _authorization.OnNicknameError -= OnNicknameRejected;
        }

        public override void Show()
        {
            base.Show();
            ShowNicknameStep();
        }

        private void ShowNicknameStep()
        {
            _nicknamePanel.SetActive(true);
            _skinPanel.SetActive(false);
            _nicknameInput.text = string.Empty;
            _errorText.text = string.Empty;
            _nextButton.interactable = true;
            _selectedSkinId = 0;
        }

        private void OnNext()
        {
            var nickname = _nicknameInput.text.Trim();

            if (nickname.Length < MinLength || nickname.Length > MaxLength)
            {
                _errorText.text = $"Никнейм должен быть от {MinLength} до {MaxLength} символов";
                return;
            }

            if (!AllowedChars.IsMatch(nickname))
            {
                _errorText.text = "Разрешены только буквы, цифры, пробел, _ и -";
                return;
            }

            if (ProfanityFilter.ContainsProfanity(nickname))
            {
                _errorText.text = "Никнейм содержит недопустимые слова";
                return;
            }

            _nickname = nickname;
            _errorText.text = string.Empty;
            _nextButton.interactable = false;
            _startButton.interactable = false;
            _authorization.ValidateNickname(nickname);
        }

        private void OnNicknameValidated()
        {
            _nicknamePanel.SetActive(false);
            _skinPanel.SetActive(true);
        }

        private void OnNicknameRejected(string reason)
        {
            _errorText.text = reason;
            _nextButton.interactable = true;
        }

        private void SelectSkin(int skinId)
        {
            _selectedSkinId = skinId;
            _startButton.interactable = true;

            switch (_selectedSkinId)
            {
                case 0:
                    _skin0Bg.color = Color.green;
                    _skin1Bg.color = Color.white;
                    _skin0Button.interactable = false;
                    _skin1Button.interactable = true;
                    break;
                case 1:
                    _skin0Bg.color = Color.white;
                    _skin1Bg.color = Color.green;
                    _skin0Button.interactable = true;
                    _skin1Button.interactable = false;
                    break;
            }
        }

        private void OnStart()
        {
            _authorization.Registration(_nickname, _selectedSkinId);
            Hide();
        }
    }
}
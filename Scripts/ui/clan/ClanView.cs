using infrastructure.services.clan;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.clan
{
    public class ClanView : Popup
    {
        [SerializeField] private NoClanView _noClanView;
        [SerializeField] private ClanListView _clanListView;
        [SerializeField] private CurrentClanView _currentClanView;

        [SerializeField] private Button _clanButton;
        [SerializeField] private Button _clanListButton;
        [SerializeField] private Button _closeButton;

        private IClanService _clanService;
        
        [Inject]
        public void Construct(IClanService clanService)
        {
            _clanService = clanService;

            _clanService.OnClanCreated += OnClanCreated;
        }

        public override void Initialize()
        {
            _noClanView.Initialize();
            _clanListView?.Initialize();
            _currentClanView.Initialize();
            
            _clanButton.onClick.AddListener(ShowClan);
            _clanListButton.onClick.AddListener(ShowClanList);
            
            _closeButton.onClick.AddListener(Hide);
        }

        public override void Show()
        {
            if (_clanService.IsInClan)
            {
                _currentClanView.Show();
            }
            else
            {
                _noClanView.Show();
            }
            
            base.Show();
        }

        public override void Hide()
        {
            _noClanView.Hide();
            _clanListView?.Hide();
            _currentClanView.Hide();
            base.Hide();
        }

        private void ShowClan()
        {
            if (_clanService.IsInClan)
            {
                _currentClanView.Show();
            }
            else
            {
                _noClanView.Show();
            }
            _clanListView?.Hide();
        }

        private void ShowClanList()
        {
            if (_clanService.IsInClan)
            {
                _currentClanView.Hide();
            }
            else
            {
                _noClanView.Hide();
            }
            _clanListView?.Show();
        }

        private void OnClanCreated(bool success, int clanId, ClanData clanData)
        {
            if (!success) return;
            _noClanView.Hide();
            _currentClanView.Show();
        }
    }
}
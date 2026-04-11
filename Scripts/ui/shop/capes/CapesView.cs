using System.Collections.Generic;
using System.Linq;
using System.Threading;
using character;
using Cysharp.Threading.Tasks;
using infrastructure.factories;
using infrastructure.services.players;
using infrastructure.services.shop;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.shop.capes
{
    public class CapesView : Popup
    {
        [SerializeField] private Transform _capesParent;
        [SerializeField] private CharacterSkinPreview _previewCharacterSkin;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;

        private const int PageSize = 9;
        private int _currentPage;
        private int _totalPages;
        private List<int> _sortedCapeIds;
        private readonly List<CapeElement> _currentPageElements = new();
        private CapeElement _currentEquipedCapeElement;
        private CapeElement _currentEquipedOnPreviewCapeElement;
        private CancellationTokenSource _pageLoadCts;

        private IShopService _shopService;
        private ICharacterService _characterService;

        [Inject]
        public void Construct(IShopService shopService, ICharacterService characterService)
        {
            _shopService = shopService;
            _characterService = characterService;

            _shopService.OnAvailableCapesLoaded += OnAvailableCapesLoaded;
            _shopService.OnAvailableCapeAdded += OnCapeBecameAvailable;
            _shopService.OnCapeEquiped += SetEquippedCape;
        }

        public override void Initialize()
        {
            Pool.CreatePool<CapeElement>(9);
            _prevButton.onClick.AddListener(GoToPrevPage);
            _nextButton.onClick.AddListener(GoToNextPage);
            CapeElement.OnPreviewClick += PreviewCape;
        }

        public override void Show()
        {
            if (_sortedCapeIds == null)
            {
                _sortedCapeIds = _shopService.Capes.Keys.OrderBy(id => id).ToList();
                _totalPages = Mathf.CeilToInt((float)_sortedCapeIds.Count / PageSize);
                _currentPage = 0;
                _shopService.GetCapes();
            }

            _previewCharacterSkin.SetCape(_characterService.CurrentCharacter.CharacterSkin.CapeId);

            ShowCurrentPage();
            base.Show();
        }

        public override void Hide()
        {
            _pageLoadCts?.Cancel();
            base.Hide();
        }

        private void ShowCurrentPage()
        {
            _pageLoadCts?.Cancel();
            _pageLoadCts?.Dispose();
            _pageLoadCts = new CancellationTokenSource();

            foreach (var el in _currentPageElements)
                el.Release();

            _currentPageElements.Clear();
            _currentEquipedCapeElement = null;
            _currentEquipedOnPreviewCapeElement = null;

            _prevButton.interactable = _currentPage > 0;
            _nextButton.interactable = _currentPage < _totalPages - 1;

            int startIndex = _currentPage * PageSize;
            int count = Mathf.Min(PageSize, _sortedCapeIds.Count - startIndex);
            var pageIds = _sortedCapeIds.GetRange(startIndex, count);

            foreach (var _ in pageIds)
            {
                var element = Pool.Get<CapeElement>();
                element.transform.SetParent(_capesParent, false);
                _currentPageElements.Add(element);
            }

            LoadPageAsync(pageIds, _pageLoadCts.Token).Forget();
        }

        private async UniTaskVoid LoadPageAsync(List<int> capeIds, CancellationToken ct)
        {
            for (int i = 0; i < capeIds.Count; i++)
            {
                if (ct.IsCancellationRequested) return;

                var element = _currentPageElements[i];
                var capeId = capeIds[i];
                var capeData = await _shopService.GetCapeDataAsync(capeId);

                if (ct.IsCancellationRequested) return;
                if (capeData == null) continue;

                element.SetData(capeData, _shopService.Capes[capeId]);

                if (_characterService.CurrentCharacter.CharacterSkin.CapeId == capeId)
                {
                    element.SetEquiped();
                    _currentEquipedCapeElement = element;
                    if (_currentEquipedOnPreviewCapeElement == null)
                    {
                        _currentEquipedOnPreviewCapeElement = element;
                        element.SetEquipPreviewCape();
                    }
                }
                else if (_shopService.AvailableCapes.Contains(capeId))
                {
                    element.SetAvailable();
                }
            }
        }

        private void GoToPrevPage()
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                ShowCurrentPage();
            }
        }

        private void GoToNextPage()
        {
            if (_currentPage < _totalPages - 1)
            {
                _currentPage++;
                ShowCurrentPage();
            }
        }

        private void PreviewCape(CapeElement capeElement)
        {
            _currentEquipedOnPreviewCapeElement?.SetUnequipPreviewCape();
            _previewCharacterSkin.SetCape(capeElement.Data.Id);
            _currentEquipedOnPreviewCapeElement = capeElement;
            _currentEquipedOnPreviewCapeElement.SetEquipPreviewCape();
        }

        private void SetEquippedCape(int capeId)
        {
            _currentEquipedCapeElement?.SetUnequiped();
            _currentEquipedCapeElement = null;
            int pageStart = _currentPage * PageSize;
            for (int i = 0; i < _currentPageElements.Count; i++)
            {
                if (_sortedCapeIds[pageStart + i] == capeId)
                {
                    _currentEquipedCapeElement = _currentPageElements[i];
                    _currentEquipedCapeElement.SetEquiped();
                    return;
                }
            }
        }

        private void OnAvailableCapesLoaded()
        {
            int pageStart = _currentPage * PageSize;
            for (int i = 0; i < _currentPageElements.Count; i++)
            {
                var capeId = _sortedCapeIds[pageStart + i];
                if (_shopService.AvailableCapes.Contains(capeId))
                    _currentPageElements[i].SetAvailable();
            }
        }

        private void OnCapeBecameAvailable(int capeId)
        {
            int pageStart = _currentPage * PageSize;
            for (int i = 0; i < _currentPageElements.Count; i++)
            {
                if (_sortedCapeIds[pageStart + i] == capeId)
                {
                    _currentPageElements[i].SetAvailable();
                    return;
                }
            }
        }
    }
}

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

namespace ui.shop.skins
{
    public class SkinsView : Popup
    {
        [SerializeField] private Transform _skinsParent;
        [SerializeField] private CharacterSkinPreview _previewCharacterSkin;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;

        private const int PageSize = 9;
        private int _currentPage;
        private int _totalPages;
        private List<int> _sortedSkinIds;
        private readonly List<SkinElement> _currentPageElements = new();
        private SkinElement _currentEquipedSkinElement;
        private SkinElement _currentEquipedOnPreviewSkinElement;
        private CancellationTokenSource _pageLoadCts;

        private IShopService _shopService;
        private ICharacterService _characterService;

        [Inject]
        public void Construct(IShopService shopService, ICharacterService characterService)
        {
            _shopService = shopService;
            _characterService = characterService;

            _shopService.OnAvailableSkinsLoaded += OnAvailableSkinsLoaded;
            _shopService.OnAvailableSkinAdded += OnSkinBecameAvailable;
            _shopService.OnSkinEquiped += SetEquippedSkin;
            // _characterService.OnSkinChanged += SetEquippedSkin;
        }

        public override void Initialize()
        {
            Pool.CreatePool<SkinElement>(9);
            _prevButton.onClick.AddListener(GoToPrevPage);
            _nextButton.onClick.AddListener(GoToNextPage);
            SkinElement.OnPreviewClick += PreviewSkin;
        }

        public override void Show()
        {
            if (_sortedSkinIds == null)
            {
                _sortedSkinIds = _shopService.Skins.Keys.OrderBy(id => id).ToList();
                _totalPages = Mathf.CeilToInt((float)_sortedSkinIds.Count / PageSize);
                _currentPage = 0;
                _shopService.GetSkins();
            }

            _previewCharacterSkin.SetSkin(_characterService.CurrentCharacter.CharacterSkin.SkinId);

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
            _currentEquipedSkinElement = null;
            _currentEquipedOnPreviewSkinElement = null;

            _prevButton.interactable = _currentPage > 0;
            _nextButton.interactable = _currentPage < _totalPages - 1;

            int startIndex = _currentPage * PageSize;
            int count = Mathf.Min(PageSize, _sortedSkinIds.Count - startIndex);
            var pageIds = _sortedSkinIds.GetRange(startIndex, count);

            foreach (var _ in pageIds)
            {
                var element = Pool.Get<SkinElement>();
                element.transform.SetParent(_skinsParent, false);
                _currentPageElements.Add(element);
            }

            LoadPageAsync(pageIds, _pageLoadCts.Token).Forget();
        }

        private async UniTaskVoid LoadPageAsync(List<int> skinIds, CancellationToken ct)
        {
            for (int i = 0; i < skinIds.Count; i++)
            {
                if (ct.IsCancellationRequested) return;

                var element = _currentPageElements[i];
                var skinId = skinIds[i];
                var skinData = await _shopService.GetSkinDataAsync(skinId);

                if (ct.IsCancellationRequested) return;
                if (skinData == null) continue;

                element.SetData(skinData, _shopService.Skins[skinId]);

                if (_characterService.CurrentCharacter.CharacterSkin.SkinId == skinId)
                {
                    element.SetEquiped();
                    _currentEquipedSkinElement = element;
                    if (_currentEquipedOnPreviewSkinElement == null)
                    {
                        _currentEquipedOnPreviewSkinElement = element;
                        element.SetEquipPreviewSkin();
                    }
                }
                else if (_shopService.AvailableSkins.Contains(skinId))
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

        private void PreviewSkin(SkinElement skinElement)
        {
            _currentEquipedOnPreviewSkinElement?.SetUnequipPreviewSkin();
            _previewCharacterSkin.SetSkin(skinElement.Data.Id);
            _currentEquipedOnPreviewSkinElement = skinElement;
            _currentEquipedOnPreviewSkinElement.SetEquipPreviewSkin();
        }

        private void SetEquippedSkin(int skinId)
        {
            _currentEquipedSkinElement?.SetUnequiped();
            _currentEquipedSkinElement = null;
            int pageStart = _currentPage * PageSize;
            for (int i = 0; i < _currentPageElements.Count; i++)
            {
                if (_sortedSkinIds[pageStart + i] == skinId)
                {
                    _currentEquipedSkinElement = _currentPageElements[i];
                    _currentEquipedSkinElement.SetEquiped();
                    return;
                }
            }
        }

        private void OnAvailableSkinsLoaded()
        {
            int pageStart = _currentPage * PageSize;
            for (int i = 0; i < _currentPageElements.Count; i++)
            {
                var skinId = _sortedSkinIds[pageStart + i];
                if (_shopService.AvailableSkins.Contains(skinId))
                    _currentPageElements[i].SetAvailable();
            }
        }

        private void OnSkinBecameAvailable(int skinId)
        {
            int pageStart = _currentPage * PageSize;
            for (int i = 0; i < _currentPageElements.Count; i++)
            {
                if (_sortedSkinIds[pageStart + i] == skinId)
                {
                    _currentPageElements[i].SetAvailable();
                    return;
                }
            }
        }
    }
}
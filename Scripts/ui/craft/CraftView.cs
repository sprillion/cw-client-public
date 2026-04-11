using System.Collections.Generic;
using I2.Loc;
using infrastructure.factories;
using infrastructure.services.craft;
using TMPro;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.craft
{
    public class CraftView : Popup
    {
        [SerializeField] private Transform _craftsParent;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Button _closeButton;
         
        private ICraftService _craftService;
        private List<CraftDataJson> _currentCraftsData;

        private readonly List<CraftElement> _currentCraftsElements = new List<CraftElement>();
        
        
        [Inject]
        public void Construct(ICraftService craftService)
        {
            _craftService = craftService;
        }
        
        public override void Initialize()
        {
            Pool.CreatePool<CraftElement>(20);
            _closeButton.onClick.AddListener(Back);
        }

        public override void Show(Popup backPopup, params object[] args)
        {
            CraftPlaceType craftPlaceType = (CraftPlaceType)args[0];
            
            SetTitle(craftPlaceType);

            _currentCraftsData = _craftService.GetCraftsFromPlace(craftPlaceType);
            FillCrafts();

            _craftService.OnCraftsUpdated += FillCrafts;
            
            base.Show(backPopup, args);
        }

        public override void Hide()
        {
            _craftService.OnCraftsUpdated -= FillCrafts;
            base.Hide();
        }

        private void SetTitle(CraftPlaceType craftPlaceType)
        {
            _titleText.text = LocalizationManager.GetTranslation($"House/{craftPlaceType}");
        }

        private void FillCrafts()
        {
            Debug.Log("UPDATE CRAFTS");
            ClearCrafts();

            foreach (var craftData in _currentCraftsData)
            {
                CreateCraft(craftData);
            }
        }

        private void ClearCrafts()
        {
            foreach (var element in _currentCraftsElements)
            {
                element.Release();
            }
        }

        private void CreateCraft(CraftDataJson craftData)
        {
            var element = Pool.Get<CraftElement>();
            element.SetParentPreserveScale(_craftsParent);
            element.SetData(craftData);
            _currentCraftsElements.Add(element);
        }
    }
}
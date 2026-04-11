using infrastructure.services.craft;
using ui.craft;
using UnityEngine;
using UnityEngine.UI;
using ui.popup;

namespace ui.house
{
    public class HouseView : Popup
    {
        [SerializeField] private CraftView _craftView;
        
        [SerializeField] private Button _workbenchButton;
        [SerializeField] private Button _furnaceButton;
        [SerializeField] private Button _anvilButton;
        [SerializeField] private Button _potionMakerButton;

        [SerializeField] private Button _closeButton;

        public override void Initialize()
        {
            _craftView.Initialize();
            
            _workbenchButton.onClick.AddListener(() => ShowCraftView(CraftPlaceType.Workbench));
            _furnaceButton.onClick.AddListener(() => ShowCraftView(CraftPlaceType.Furnace));
            _anvilButton.onClick.AddListener(() => ShowCraftView(CraftPlaceType.Anvil));
            _potionMakerButton.onClick.AddListener(() => ShowCraftView(CraftPlaceType.PotionMaker));
            
            _closeButton.onClick.AddListener(Hide);
        }

        private void ShowCraftView(CraftPlaceType craftPlaceType)
        {
            _craftView.Show(this, craftPlaceType);
        }
        
    }
}
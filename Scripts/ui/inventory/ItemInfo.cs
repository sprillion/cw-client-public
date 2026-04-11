using infrastructure.factories;
using infrastructure.services.inventory.items;
using TMPro;
using UnityEngine;

namespace ui.inventory
{
    public class ItemInfo : MonoBehaviour
    {
        [SerializeField] private Slot _slot;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;

        private Item _currentItem;

        private UiItem _uiItem;

        public void Initialize()
        {
            _uiItem = Pool.Get<UiItem>();
            _uiItem.Draggable = false;
            _slot.SetVisualItem(_uiItem);
            UiItem.OnItemSelected += SetItem;
        }

        private void OnDestroy()
        {
            UiItem.OnItemSelected -= SetItem;
        }

        public void SetItem(UiItem uiItem)
        {
            if (uiItem.CurrentItem == _currentItem) return;

            _currentItem = uiItem.CurrentItem;
            
            _uiItem.Initialize(_currentItem, null, false);
            
            _title.text = $"Items/{_currentItem.Id}".Loc();
            _description.text = $"ItemsDescription/{_currentItem.Id}".Loc();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
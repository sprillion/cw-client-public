using System;
using infrastructure.services.inventory.items;
using TMPro;
using ui.inventory.equipSlot;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ui.inventory
{
    public class UiItem : PooledObject, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _countText;

        private Transform _movingParent;
        
        private Transform _currentParent;

        private bool _parentChanged;

        public bool Draggable { get; set; } = true;

        public Item CurrentItem { get; private set; }

        public event Action<UiItem> OnClickItem; 
        public event Action OnRemove;

        public static event Action<UiItem> OnItemSelected; 
        
        public void Initialize(Item item, Transform movingParent, bool interactable = true)
        {
            CurrentItem = item;
            _movingParent = movingParent;
            _image.sprite = CurrentItem.Data.Icon;
            _image.raycastTarget = interactable;
            CurrentItem.OnCountChange += SetCount;
            _countText.gameObject.SetActive(CurrentItem.EquipSlotType is EquipSlotType.None);
            SetCount();
        }

        public override void Release()
        {
            OnClickItem = null;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            CurrentItem.OnCountChange -= SetCount;
            OnRemove?.Invoke();
            base.Release();
        }

        public void SetParent(Transform parent)
        {
            SetParentPreserveScale(parent);
            transform.localPosition = Vector3.zero;
            _parentChanged = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnItemSelected?.Invoke(this);
            
            if (!Draggable) return;
            
            _currentParent = transform.parent;
            SetParentPreserveScale(_movingParent);
            transform.SetAsLastSibling();
            _canvasGroup.alpha = 0.6f;
            _canvasGroup.blocksRaycasts = false;
            _parentChanged = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Draggable) return;
            transform.position = Mouse.current.position.value;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Draggable) return;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;

            if (_parentChanged) return;
            
            SetParentPreserveScale(_currentParent);
            transform.localPosition = Vector3.zero;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnItemSelected?.Invoke(this);
            
            if (Draggable) return;
            
            OnClickItem?.Invoke(this);
        }
        
        private void SetCount()
        {
            _countText.text = CurrentItem?.Count.ToString();
        }
    }
}
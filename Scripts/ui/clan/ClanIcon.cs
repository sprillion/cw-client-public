using System;
using UnityEngine;
using UnityEngine.UI;

namespace ui.clan
{
    public class ClanIcon : PooledObject
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Button _button;

        [SerializeField] private GameObject _selected;

        public static event Action<ClanIcon> OnSelected;

        public int Id { get; private set; }
        
        private void Awake()
        {
            _button.onClick.AddListener(Select);
        }

        public void SetIcon(Sprite sprite, int id)
        {
            _icon.sprite = sprite;
            Id = id;
        }

        public void Unselect()
        {
            _selected.SetActive(false);
            _button.interactable = true;
        }
        
        public void Select()
        {
            _selected.SetActive(true);
            _button.interactable = false;
            
            OnSelected?.Invoke(this);
        }
    }
}
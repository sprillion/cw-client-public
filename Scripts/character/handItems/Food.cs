using infrastructure.services.inventory.items;
using UnityEngine;

namespace character.handItems
{
    public class Food : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private Item _currentItem;

        public void SetFood(Item item)
        {
            _spriteRenderer.sprite = item.Data.Icon;
        }
    }
}
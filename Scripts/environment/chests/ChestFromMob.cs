using System;
using System.Collections.Generic;
using infrastructure.services.chests;
using infrastructure.services.inventory.items;
using infrastructure.services.ui;
using Sirenix.Utilities;
using UnityEngine;
using Zenject;

namespace environment.chests
{
    public class ChestFromMob : PooledObject, IInteractable
    {
        private IChestsService _chestsService;
        private IUiService _uiService;
        
        public ushort Id { get; set; }

        public List<Item> Items { get; set; } = new List<Item>();

        public event Action OnSetItems;
        public event Action<IInteractable> OnDestroyed;
        
        [Inject]
        public void Construct(IChestsService chestsService)
        {
            _chestsService = chestsService;
        }

        public override void Release()
        {
            Items?.Clear();
            OnSetItems = null;
            OnDestroyed?.Invoke(this);
            OnDestroyed = null;
            base.Release();
        }

        public void Interact()
        {
            if (Items.IsNullOrEmpty())
            {
                _chestsService.OpenChest(Id);
            }
        }

        public void SetItems(List<Item> items)
        {
            Items?.Clear();
            Items?.AddRange(items);

            OnSetItems?.Invoke();
        }
    }
}
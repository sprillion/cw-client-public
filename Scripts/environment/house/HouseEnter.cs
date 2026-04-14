using System;
using UnityEngine;

namespace environment.house
{
    public class HouseEnter : PooledObject, IInteractable
    {
        [field: SerializeField] public bool DisablePanel { get; private set; }
        [field: SerializeField] public bool DisableButton { get; private set; }
        
        public event Action<IInteractable> OnDestroyed;
        
        public void Interact()
        {
            
        }
    }
}
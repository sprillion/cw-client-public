using System;
using UnityEngine;

namespace environment.house
{
    public class HouseEnter : PooledObject, IInteractable
    {
        public void Interact()
        {
            
        }

        public event Action<IInteractable> OnDestroyed;
    }
}
using System;
using UnityEngine;

namespace environment.mine
{
    public class MineEnter : PooledObject, IInteractable
    {
        public void Interact()
        {
            
        }

        public event Action<IInteractable> OnDestroyed;
    }
}
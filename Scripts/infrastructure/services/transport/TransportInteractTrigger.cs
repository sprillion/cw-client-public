using System;
using environment;
using UnityEngine;

namespace infrastructure.services.transport
{
    public class TransportInteractTrigger : MonoBehaviour, IInteractable
    {
        private Transport _transport;
        
        [field: SerializeField] public bool DisablePanel { get; private set; }
        [field: SerializeField] public bool DisableButton { get; private set; }
        public event Action<IInteractable> OnDestroyed;

        public void Initialize(Transport transport)
        {
            _transport = transport;
        }

        public void Interact() => _transport?.OnInteractRequested?.Invoke();

        // Вызывается из Transport.Release() — оповещает Interaction об удалении объекта
        // на случай, если персонаж находится внутри триггера в момент возврата в пул.
        public void NotifyDestroyed() => OnDestroyed?.Invoke(this);
    }
}

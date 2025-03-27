using System;

namespace environment
{
    public interface IInteractable
    {
        void Interact();

        event Action<IInteractable> OnDestroyed;
    }
}
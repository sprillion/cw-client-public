using System;

namespace environment
{
    public interface IInteractable
    {
        bool DisablePanel { get; }
        bool DisableButton { get; }
        event Action<IInteractable> OnDestroyed;
        void Interact();

    }
}
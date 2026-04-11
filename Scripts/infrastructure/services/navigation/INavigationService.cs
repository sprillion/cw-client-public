using System;
using network;
using UnityEngine;

namespace infrastructure.services.navigation
{
    public interface INavigationService : IReceiver
    {
        bool IsActive { get; }

        event Action OnNavigationStarted;
        event Action OnNavigationCompleted;
        event Action OnNavigationCancelled;
        event Action<float> OnCooldownRejected;
        event Action OnNotOnTransport;
        event Action OnPathPartial;
        event Action OnPathInvalid;

        void RequestPath(Vector3 target);
        void CancelNavigation();
    }
}

using System;
using Cysharp.Threading.Tasks;

namespace infrastructure.services.interior
{
    public interface IInteriorService
    {
        bool IsInInterior { get; }
        event Action OnEntered;
        event Action OnExited;

        UniTask Enter(InteriorType interiorType);
        void Exit();
    }
}

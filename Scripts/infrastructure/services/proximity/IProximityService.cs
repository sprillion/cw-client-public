using UnityEngine;

namespace infrastructure.services.proximity
{
    public interface IProximityService
    {
        void Register(GameObject obj);
        void Unregister(GameObject obj);
    }
}

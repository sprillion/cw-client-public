using System;
using UnityEngine;

namespace infrastructure.services.waypoint
{
    public interface IWaypointService
    {
        Vector3? WaypointPosition { get; }
        event Action<Vector3?> OnWaypointChanged;
        void SetWaypoint(Vector3 position);
        void ClearWaypoint();
    }
}

using System;
using UnityEngine;

namespace infrastructure.services.waypoint
{
    public class WaypointService : IWaypointService
    {
        public Vector3? WaypointPosition { get; private set; }
        public event Action<Vector3?> OnWaypointChanged;

        public void SetWaypoint(Vector3 position)
        {
            WaypointPosition = position;
            OnWaypointChanged?.Invoke(position);
        }

        public void ClearWaypoint()
        {
            WaypointPosition = null;
            OnWaypointChanged?.Invoke(null);
        }
    }
}

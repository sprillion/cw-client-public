using System;
using UnityEngine;

namespace infrastructure.services.map
{
    public interface IMapService
    {
        event Action ChunksUpdated;
        event Action<bool> OnMapLoaded;
        void LoadMap();
        void CreateStartChunks(Vector3 position);
        void LaunchCheckPosition();
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace infrastructure.services.map
{
    public interface IMapService
    {
        Dictionary<Vector2Short, ChunkRenderer> Chunks { get; }
        event Action ChunksUpdated;
        event Action<bool> OnMapLoaded;
        BlockType GetBlockType(Vector3 worldPos);
        float? GetSurfaceHeight(float worldX, float worldZ);
        void LoadMap();
        void CreateStartChunks(Vector3 position);
        void LaunchCheckPosition();
        void SetViewerOverride(Transform viewer);
        void ClearViewerOverride();
    }
}
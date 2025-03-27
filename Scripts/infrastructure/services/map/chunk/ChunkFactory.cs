using UnityEngine;
using Zenject;

namespace infrastructure.services.map.chunk
{
    public class ChunkFactory : IChunkFactory
    {
        private const string ChunkPrefabPath = "Prefabs/Map/Chunk";
        private const string WaterPrefabPath = "Prefabs/Map/Water";

        private readonly DiContainer _container;

        private ChunkRenderer _chunkRendererPrefab;
        private WaterRenderer _waterRendererPrefab;

        private readonly ObjectPool _chunkPool;
        private readonly ObjectPool _waterPool;

        public ChunkFactory(DiContainer diContainer)
        {
            _container = diContainer;
            _chunkRendererPrefab = Resources.Load<ChunkRenderer>(ChunkPrefabPath);
            _waterRendererPrefab = Resources.Load<WaterRenderer>(WaterPrefabPath);
            _chunkPool = new ObjectPool(_chunkRendererPrefab, 9);
            _waterPool = new ObjectPool(_waterRendererPrefab, 9);
        }

        public ChunkRenderer GetChunk()
        {
            var chunk = _chunkPool.GetObject<ChunkRenderer>();
            _container.Inject(chunk);
            return chunk;
        }

        public WaterRenderer GetWater()
        {
            var water = _waterPool.GetObject<WaterRenderer>();
            _container.Inject(water);
            return water;
        }
    }
}
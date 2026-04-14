using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using infrastructure.factories;
using infrastructure.services.bundles;
using infrastructure.services.map.chunk;
using infrastructure.services.map.import;
using infrastructure.services.players;
using infrastructure.services.settings;
using Sirenix.Utilities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Zenject;

namespace infrastructure.services.map
{
    public class MapService : MonoBehaviour, IMapService
    {
        private int _renderRadius = 5;

        private ICharacterService _characterService;
        private IBundleService _bundleService;

        private Vector2Short _currentChunkPosition;
        private bool _updated;
        private bool _canCheckPosition;
        private Transform _viewerOverride;

        private Material _worldMaterial;
        private Material _waterMaterial;

        private readonly Dictionary<Vector2Short, ChunkRenderer> _chunks = new Dictionary<Vector2Short, ChunkRenderer>();
        private readonly List<ChunkRenderer> _newChunks = new List<ChunkRenderer>(32);

        public Dictionary<Vector2Short, ChunkData> ChunkDatas { get; private set; } = new Dictionary<Vector2Short, ChunkData>();

        public Dictionary<Vector2Short, ChunkRenderer> Chunks => _chunks;
        public event Action ChunksUpdated;
        public event Action<bool> OnMapLoaded;

        [Inject]
        public void Construct(ICharacterService characterService, IBundleService bundleService, ISettingsService settingsService)
        {
            _characterService = characterService;
            _bundleService = bundleService;
            _renderRadius = settingsService.Current.renderDistance;
            settingsService.OnRenderDistanceChanged += r => _renderRadius = r;
        }

        private void Update()
        {
            if (_updated) return;
            CheckPlayerPosition();
        }

        public void LoadMap()
        {
            LoadMapCoroutine().Forget();
        }

        public void CreateStartChunks(Vector3 position)
        {
            CreateChunksFromPosition(position, true);
        }

        public void LaunchCheckPosition()
        {
            _canCheckPosition = true;
        }

        public BlockType GetBlockType(Vector3 worldPos)
        {
            int wx = Mathf.FloorToInt(worldPos.x);
            int wy = Mathf.FloorToInt(worldPos.y);
            int wz = Mathf.FloorToInt(worldPos.z);

            short chunkX = (short)(wx >= 0
                ? wx / ChunkRenderer.ChunkWidth
                : (wx - ChunkRenderer.ChunkWidth + 1) / ChunkRenderer.ChunkWidth);
            short chunkZ = (short)(wz >= 0
                ? wz / ChunkRenderer.ChunkWidth
                : (wz - ChunkRenderer.ChunkWidth + 1) / ChunkRenderer.ChunkWidth);

            if (!ChunkDatas.TryGetValue(new Vector2Short(chunkX, chunkZ), out var chunk))
                return BlockType.Air;

            var localPos = new Vector3Short(
                wx - chunkX * ChunkRenderer.ChunkWidth,
                wy,
                wz - chunkZ * ChunkRenderer.ChunkWidth);

            foreach (var block in chunk.Blocks)
                if (block.Position.Equals(localPos)) return block.BlockType;

            return BlockType.Air;
        }

        public float? GetSurfaceHeight(float worldX, float worldZ)
        {
            int wx = Mathf.FloorToInt(worldX);
            int wz = Mathf.FloorToInt(worldZ);

            short chunkX = (short)(wx >= 0
                ? wx / ChunkRenderer.ChunkWidth
                : (wx - ChunkRenderer.ChunkWidth + 1) / ChunkRenderer.ChunkWidth);
            short chunkZ = (short)(wz >= 0
                ? wz / ChunkRenderer.ChunkWidth
                : (wz - ChunkRenderer.ChunkWidth + 1) / ChunkRenderer.ChunkWidth);

            if (!ChunkDatas.TryGetValue(new Vector2Short(chunkX, chunkZ), out var chunk))
                return null;

            short localX = (short)(wx - chunkX * ChunkRenderer.ChunkWidth);
            short localZ = (short)(wz - chunkZ * ChunkRenderer.ChunkWidth);

            int maxY = -1;
            foreach (var block in chunk.Blocks)
            {
                if (block.Position.X == localX && block.Position.Z == localZ && block.Position.Y > maxY)
                    maxY = block.Position.Y;
            }

            return maxY >= 0 ? (float)(maxY + 1) : (float?)null;
        }

        private ChunkData GetChunkData(short x, short y)
        {
            return ChunkDatas.GetValueOrDefault(new Vector2Short(x, y));
        }

        // Instantiates a chunk from the pool and sets data, but does NOT generate the mesh.
        // Call ScheduleMeshJob() / ApplyMeshResults() separately for batch generation.
        private ChunkRenderer InstantiateChunk(short x, short y)
        {
            var chunkData = GetChunkData(x, y);
            if (chunkData == null) return null;

            float xPos = x * ChunkRenderer.ChunkWidth;
            float zPos = y * ChunkRenderer.ChunkWidth;

            var chunk = Pool.Get<ChunkRenderer>();
            chunk.transform.position = new Vector3(xPos, 0, zPos);
            chunk.transform.SetParent(transform);
            chunk.SetData(chunkData, _worldMaterial, _waterMaterial);
            return chunk;
        }

        private async UniTaskVoid LoadMapCoroutine()
        {
            Debug.Log("[MapService] Loading map bundle...");
            var map = await _bundleService.LoadMapAsync();
            Debug.Log("[MapService] Loading WorldMaterial...");
            _worldMaterial = await _bundleService.LoadMaterialAsync("WorldMaterial");
            Debug.Log("[MapService] Loading WaterMaterial...");
            _waterMaterial = await _bundleService.LoadMaterialAsync("WaterMaterial");
            if (map.IsNullOrEmpty())
            {
                Debug.LogError("[MapService] Map data is empty or null — invoking OnMapLoaded(false).");
                OnMapLoaded?.Invoke(false);
            }
            else
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                Debug.Log($"[MapService] Deserializing map ({map.Length} bytes) in batches (WebGL)...");
                await DeserializeMapAsync(map);
#else
                Debug.Log($"[MapService] Deserializing map ({map.Length} bytes) on thread pool...");
                await UniTask.RunOnThreadPool(() => DeserializeMap(map));
#endif
                Debug.Log($"[MapService] Map deserialized: {ChunkDatas.Count} chunks.");
                OnMapLoaded?.Invoke(true);
            }
        }

        private void DeserializeMap(byte[] data)
        {
            var chunks = Serializer.Deserialize(data);

            ChunkDatas = chunks.ToDictionary(chunk => chunk.Position, chunk =>
            {
                return new ChunkData()
                {
                    Position = chunk.Position,
                    Blocks = chunk.Blocks.ConvertAll<RenderedBlock>(c =>
                    {
                        BoolIntConverter.UnpackBoolsAndNumber(c.Value, out var faces);
                        return new RenderedBlock()
                        {
                            Position  = c.Position,
                            BlockType = c.BlockType,
                            Faces     = faces,
                            Rotate    = c.Rotation
                        };
                    })
                };
            });
        }

        // Батчевая десериализация для WebGL (нет потоков).
        // Читает по batchSize чанков за кадр, чтобы не подвешивать браузер.
        private async UniTask DeserializeMapAsync(byte[] data, int batchSize = 10)
        {
            ChunkDatas = new Dictionary<Vector2Short, ChunkData>();
            int count = 0;
            foreach (var chunk in Serializer.DeserializeLazy(data))
            {
                ChunkDatas[chunk.Position] = new ChunkData
                {
                    Position = chunk.Position,
                    Blocks = chunk.Blocks.ConvertAll<RenderedBlock>(c =>
                    {
                        BoolIntConverter.UnpackBoolsAndNumber(c.Value, out var faces);
                        return new RenderedBlock
                        {
                            Position  = c.Position,
                            BlockType = c.BlockType,
                            Faces     = faces,
                            Rotate    = c.Rotation
                        };
                    })
                };
                count++;
                if (count % batchSize == 0)
                    await UniTask.Yield();
            }
        }

        public void SetViewerOverride(Transform viewer) => _viewerOverride = viewer;
        public void ClearViewerOverride()               => _viewerOverride = null;

        private void CheckPlayerPosition()
        {
            if (!_canCheckPosition) return;
            var position = _viewerOverride != null
                ? _viewerOverride.position
                : _characterService?.CurrentCharacter?.transform.position;
            if (position == null) return;
            CreateChunksFromPosition(position.Value);
        }

        private void CreateChunksFromPosition(Vector3 position, bool force = false)
        {
            var currentChunkPosition = new Vector2Short(
                (short)(position.x / ChunkRenderer.ChunkWidth),
                (short)(position.z / ChunkRenderer.ChunkWidth)
            );
            if (currentChunkPosition == _currentChunkPosition && !force) return;
            _currentChunkPosition = currentChunkPosition;

            UpdateChunks().Forget();
        }

        private async UniTaskVoid UpdateChunks()
        {
            if (_updated) return;
            _updated = true;
            // Debug.Log($"[MapService] UpdateChunks started. ChunkDatas: {ChunkDatas.Count}, renderRadius: {_renderRadius}");
            _newChunks.Clear();

            var toSchedule = new List<ChunkRenderer>();

            // Pass 1: instantiate new chunks on the main thread (no mesh generation yet)
            for (int x = -_renderRadius; x <= _renderRadius; x++)
            {
                for (int z = -_renderRadius; z <= _renderRadius; z++)
                {
                    var position = _currentChunkPosition + new Vector2Short(x, z);
                    if (_chunks.Remove(position, out var existing))
                    {
                        _newChunks.Add(existing);
                        continue;
                    }

                    var newChunk = InstantiateChunk(position.X, position.Y);
                    if (newChunk == null) continue;
                    _newChunks.Add(newChunk);
                    toSchedule.Add(newChunk);
                }
            }

            // Pass 2: schedule all mesh jobs (worker threads pick them up immediately)
            // Must use Allocator.TempJob — Allocator.Temp only lives 1 frame and would be
            // invalid after the yield below.
            var handles = new NativeArray<JobHandle>(toSchedule.Count, Allocator.TempJob);
            for (int i = 0; i < toSchedule.Count; i++)
                handles[i] = toSchedule[i].ScheduleMeshJob();

            // Yield one frame so worker threads can execute the jobs.
            // On WebGL (no SharedArrayBuffer / threads) jobs run synchronously — this is safe.
            await UniTask.Yield();

            // Pass 3: complete all jobs and apply results on the main thread
            JobHandle.CompleteAll(handles);
            handles.Dispose();
            foreach (var r in toSchedule)
                r.ApplyMeshResults();

            _chunks.ForEach(chunk => chunk.Value?.Release());
            _chunks.Clear();
            _newChunks.ForEach(chunk => _chunks.TryAdd(chunk.ChunkData.Position, chunk));
            _updated = false;

            // Debug.Log($"[MapService] UpdateChunks done. Active chunks: {_chunks.Count}. Invoking ChunksUpdated.");
            ChunksUpdated?.Invoke();
        }
    }
}

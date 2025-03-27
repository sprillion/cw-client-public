using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using infrastructure.services.map.chunk;
using infrastructure.services.map.import;
using infrastructure.services.players;
using secrets;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace infrastructure.services.map
{
    public class MapService: MonoBehaviour, IMapService
    {
        private const int RenderRadius = 5;

        private IChunkFactory _chunkFactory;
        private ICharacterService _characterService;

        private Vector2Short _currentChunkPosition;
        private bool _updated;
        private bool _canCheckPosition;

        private readonly Dictionary<Vector2Short, ChunkRenderer>
            _chunks = new Dictionary<Vector2Short, ChunkRenderer>();

        private readonly List<ChunkRenderer> _newChunks = new List<ChunkRenderer>(32);
        public Dictionary<Vector2Short, ChunkData> ChunkDatas { get; private set; } =
            new Dictionary<Vector2Short, ChunkData>();

        public event Action ChunksUpdated;
        public event Action<bool> OnMapLoaded;

        [Inject]
        public void Construct(IChunkFactory chunkFactory, ICharacterService characterService)
        {
            _chunkFactory = chunkFactory;
            _characterService = characterService;
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

        private ChunkData GetChunkData(short x, short y)
        {
            return ChunkDatas.GetValueOrDefault(new Vector2Short(x, y));
        }

        private ChunkRenderer CreateChunk(short x, short y)
        {
            var chunkData = GetChunkData(x, y);
            if (chunkData == null) return null;

            float xPos = x * ChunkRenderer.ChunkWidth;
            float zPos = y * ChunkRenderer.ChunkWidth;

            var chunk = _chunkFactory.GetChunk();
            chunk.transform.position = new Vector3(xPos, 0, zPos);
            chunk.transform.SetParent(transform);
            chunk.SetData(chunkData);
            chunk.Generate();
            return chunk;
        }

        private async UniTaskVoid LoadMapCoroutine()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(SecretKey.MapFileUrl))
            {
#if UNITY_WEBGL
                webRequest.SetRequestHeader("Cache-Control", "max-age=3600, must-revalidate");
#endif
                await webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error downloading JSON: " + webRequest.error);
                    OnMapLoaded?.Invoke(false);
                }
                else
                {
                    var chunks = Serializer.Deserialize(webRequest.downloadHandler.data);
                    
                    ChunkDatas = chunks.ToDictionary(chunk => chunk.Position, chunk =>
                    {
                        return new ChunkData()
                        {
                            Position = chunk.Position,
                            Blocks = chunk.Blocks.ConvertAll<RenderedBlock>(c =>
                            {
                                var tuple = BoolIntConverter.UnpackBoolsAndNumber(c.Value);
                                return new RenderedBlock()
                                {
                                    Position = c.Position,
                                    BlockType = c.BlockType,
                                    Right = tuple.Item1,
                                    Left = tuple.Item2,
                                    Front = tuple.Item3,
                                    Back = tuple.Item4,
                                    Top = tuple.Item5,
                                    Bottom = tuple.Item6,
                                    Rotate = tuple.Item7
                                };
                            })
                        };
                    });
                    OnMapLoaded?.Invoke(true);
                }
            }
        }

        private void CheckPlayerPosition()
        {
            if (!_canCheckPosition) return;
            if (_characterService?.CurrentCharacter == null) return;
            CreateChunksFromPosition(_characterService.CurrentCharacter.transform.position);
        }

        private void CreateChunksFromPosition(Vector3 position, bool force = false)
        {
            var currentChunkPosition = new Vector2Short(
                (short)position.x / ChunkRenderer.ChunkWidth,
                (short)position.z / ChunkRenderer.ChunkWidth
            );
            if (currentChunkPosition == _currentChunkPosition && !force) return;
            _currentChunkPosition = currentChunkPosition;
            
            UpdateChunks().Forget();
        }
        
        private async UniTaskVoid UpdateChunks()
        {
            if (_updated) return;
            _updated = true;
            _newChunks.Clear();

            for (int x = -RenderRadius; x <= RenderRadius; x++)
            {
                for (int z = -RenderRadius; z <= RenderRadius; z++)
                {
                    var position = _currentChunkPosition + new Vector2Short(x, z);
                    if (_chunks.Remove(position, out var chunk))
                    {
                        _newChunks.Add(chunk);
                        continue;
                    }

                    var newChunk = CreateChunk(position.X, position.Y);
                    if (newChunk == null) continue;
                    _newChunks.Add(newChunk);
                    await UniTask.DelayFrame(1);
                }
            }

            _chunks.ForEach(chunk => chunk.Value?.Release());
            _chunks.Clear();
            _newChunks.ForEach(chunk => _chunks.TryAdd(chunk.ChunkData.Position, chunk));
            _updated = false;
            
            ChunksUpdated?.Invoke();
        }
    }
}
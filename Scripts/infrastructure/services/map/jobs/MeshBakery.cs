using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

namespace infrastructure.services.map.jobs
{
    /// <summary>
    /// Singleton MonoBehaviour that bakes managed ScriptableObject block data into
    /// persistent NativeArrays shared by all ChunkMeshJob instances.
    /// Must be bound as NonLazy so data is ready before the first ChunkRenderer is injected.
    /// </summary>
    public class MeshBakery : MonoBehaviour
    {
        // VertPositions layout:
        //   [blockType * 192 + sideIdx * 32 + rotIdx * 8 + vertIdx]
        //   192 = 6 sides * 4 rotations * 8 max-verts-per-face
        public NativeArray<BlockBakedData> BlockData;       // [256]
        public NativeArray<FaceBakedData>  FaceData;        // [256 * 6 = 1536]
        public NativeArray<float3>         VertPositions;   // [256 * 6 * 4 * 4 = 24576]
        public NativeArray<int>            TriangleIndices; // [256 * 6 * 6 = 9216]
        // RotSideOffsets[rotIdx * 6 + sideIdx] — used for rotatable block vertex lookup.
        // rotIdx 3 = identity (0 deg Y rotation).
        public NativeArray<int3>           RotSideOffsets;  // [4 * 6 = 24]

        [Inject]
        public void Construct(BlockTextureData blockTextureData)
        {
            Bake(blockTextureData);
        }

        public void Bake(BlockTextureData textureData)
        {
            if (BlockData.IsCreated) DisposeAll();

            var blockDatasDict = new Dictionary<BlockType, BlockData>();
            foreach (var bd in textureData.BlockDatas)
                if (bd != null) blockDatasDict[bd.BlockType] = bd;

            BlockData       = new NativeArray<BlockBakedData>(256, Allocator.Persistent);
            FaceData        = new NativeArray<FaceBakedData>(256 * 6, Allocator.Persistent);
            VertPositions   = new NativeArray<float3>(256 * 6 * 4 * 8, Allocator.Persistent);
            TriangleIndices = new NativeArray<int>(256 * 6 * 12, Allocator.Persistent);
            RotSideOffsets  = new NativeArray<int3>(4 * 6, Allocator.Persistent);

            // Fill RotSideOffsets — rotIdx 3 maps to Vector3.zero (identity)
            for (int r = 0; r < 4; r++)
            {
                Vector3 angle = GetRotationAngle(r);
                for (int s = 0; s < 6; s++)
                {
                    Vector3Int sv = GetSideVector((BlockSide)s, angle);
                    RotSideOffsets[r * 6 + s] = new int3(sv.x, sv.y, sv.z);
                }
            }

            // All unknown block slots default to empty (not water, not leaves, not rotatable)
            for (int i = 0; i < 256; i++)
                BlockData[i] = new BlockBakedData();

            float countInLine = textureData.CountBlocksInLine;
            float blockSize   = textureData.BlockSize;
            float texSize     = textureData.TextureSize;

            for (int blockTypeInt = 0; blockTypeInt < 256; blockTypeInt++)
            {
                var bt = (BlockType)(byte)blockTypeInt;
                if (!blockDatasDict.TryGetValue(bt, out BlockData bd)) continue;

                float2 CalcUv(int index) => new float2(
                    (index % countInLine) * blockSize / texSize,
                    (texSize - (index / (int)countInLine + 1) * blockSize) / texSize);

                BlockData[blockTypeInt] = new BlockBakedData
                {
                    UvFront     = CalcUv(bd.FrontSideIndex),
                    UvRight     = CalcUv(bd.RightSideIndex),
                    UvBack      = CalcUv(bd.BackSideIndex),
                    UvLeft      = CalcUv(bd.LeftSideIndex),
                    UvTop       = CalcUv(bd.TopSideIndex),
                    UvBottom    = CalcUv(bd.BottomSideIndex),
                    IsRotatable = bd.IsRotatableBlock,
                    IsWater     = bt == BlockType.Water,
                    IsLeaves    = bt == BlockType.Leaves,
                };

                if (bd.VoxelMeshData == null || bd.VoxelMeshData.Faces == null) continue;

                foreach (var face in bd.VoxelMeshData.Faces)
                {
                    if (face == null || face.VertDatas == null || face.Triangles == null) continue;

                    int sideIdx = (int)face.BlockSide;
                    int fdIdx   = blockTypeInt * 6 + sideIdx;
                    int triBase = blockTypeInt * 72 + sideIdx * 12;

                    for (int t = 0; t < face.Triangles.Length && t < 12; t++)
                        TriangleIndices[triBase + t] = face.Triangles[t];

                    // Pre-compute rotated vertex positions for all 4 rotations
                    for (int r = 0; r < 4; r++)
                    {
                        Vector3 angle = bd.IsRotatableBlock ? GetRotationAngle(r) : Vector3.zero;
                        int vertBase = blockTypeInt * 192 + sideIdx * 32 + r * 8;
                        for (int v = 0; v < face.VertDatas.Length && v < 8; v++)
                        {
                            Vector3 pos = face.VertDatas[v].GetRotatedPosition(angle);
                            VertPositions[vertBase + v] = new float3(pos.x, pos.y, pos.z);
                        }
                    }

                    FaceData[fdIdx] = new FaceBakedData
                    {
                        VertCount = face.VertDatas.Length,
                        TriCount  = face.Triangles.Length,
                        TriOffset = triBase,
                    };
                }
            }
        }

        private void OnDestroy() => DisposeAll();

        private void DisposeAll()
        {
            if (BlockData.IsCreated)       BlockData.Dispose();
            if (FaceData.IsCreated)        FaceData.Dispose();
            if (VertPositions.IsCreated)   VertPositions.Dispose();
            if (TriangleIndices.IsCreated) TriangleIndices.Dispose();
            if (RotSideOffsets.IsCreated)  RotSideOffsets.Dispose();
        }

        private static Vector3 GetRotationAngle(int rotation) => rotation switch
        {
            0 => new Vector3(0f, 90f, 0f),
            1 => new Vector3(0f, 270f, 0f),
            2 => new Vector3(0f, 180f, 0f),
            3 => new Vector3(0f, 0f, 0f),
            _ => Vector3.zero
        };

        private static Vector3Int GetSideVector(BlockSide blockSide, Vector3 rotation)
        {
            Vector3 v = blockSide switch
            {
                BlockSide.Front  => Quaternion.Euler(rotation) * Vector3Int.forward,
                BlockSide.Right  => Quaternion.Euler(rotation) * Vector3Int.right,
                BlockSide.Back   => Quaternion.Euler(rotation) * Vector3Int.back,
                BlockSide.Left   => Quaternion.Euler(rotation) * Vector3Int.left,
                BlockSide.Top    => Quaternion.Euler(rotation) * Vector3Int.up,
                BlockSide.Bottom => Quaternion.Euler(rotation) * Vector3Int.down,
                _                => Vector3.zero
            };
            return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        }
    }
}

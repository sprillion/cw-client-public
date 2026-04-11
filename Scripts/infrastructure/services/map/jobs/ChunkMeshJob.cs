using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace infrastructure.services.map.jobs
{
    [BurstCompile]
    public struct ChunkMeshJob : IJob
    {
        // Pre-computed rendered blocks from the server (INPUT, TempJob)
        [ReadOnly] public NativeArray<RenderedBlockBlittable> Blocks;

        // Shared baked data from MeshBakery (Persistent, ReadOnly)
        [ReadOnly] public NativeArray<BlockBakedData> BakedBlocks;
        [ReadOnly] public NativeArray<FaceBakedData>  BakedFaces;
        [ReadOnly] public NativeArray<float3>         VertPositions;
        [ReadOnly] public NativeArray<int>            TriangleIndices;

        // When true (e.g. ChunkRendererWithLeaves), leaf faces go into separate lists.
        // When false, leaf faces go into the terrain lists like any other block.
        public bool GenerateLeaves;

        // Outputs (TempJob, pre-allocated with initial capacity)
        public NativeList<float3> TerrainVerts;
        public NativeList<float2> TerrainUvs;
        public NativeList<int>    TerrainTris;
        public NativeList<float3> WaterVerts;
        public NativeList<float2> WaterUvs;
        public NativeList<float3> LeavesVerts;
        public NativeList<float2> LeavesUvs;
        public NativeReference<int> MaxY;

        public void Execute()
        {
            int maxY = 0;

            for (int i = 0; i < Blocks.Length; i++)
            {
                RenderedBlockBlittable block = Blocks[i];
                byte blockType = block.BlockType;

                BlockBakedData bakedBlock = BakedBlocks[blockType];
                float3 blockPos = new float3(block.Position.X, block.Position.Y, block.Position.Z);

                // Water: generate a single top face at y+0.75 (always visible when block is in the list)
                if (bakedBlock.IsWater)
                {
                    WaterVerts.Add(new float3(0f, 0.75f, 0f) + blockPos);
                    WaterVerts.Add(new float3(0f, 0.75f, 1f) + blockPos);
                    WaterVerts.Add(new float3(1f, 0.75f, 0f) + blockPos);
                    WaterVerts.Add(new float3(1f, 0.75f, 1f) + blockPos);
                    WaterUvs.Add(new float2(0f, 0f));
                    WaterUvs.Add(new float2(1f, 0f));
                    WaterUvs.Add(new float2(0f, 1f));
                    WaterUvs.Add(new float2(1f, 1f));
                    continue;
                }

                // Wheat: skip rendering (special billboard rendering handled elsewhere)
                if (blockType == 38) continue;

                // Non-rotatable blocks always use rotIdx=3 (identity = 0 deg Y),
                // matching the original GetRotationAngle(3) = Vector3.zero behavior.
                int rotIdx = bakedBlock.IsRotatable ? math.clamp((int)block.Rotate, 0, 3) : 3;

                for (int sideIdx = 0; sideIdx < 6; sideIdx++)
                {
                    // Check the BlockFaces bitmask — skip faces not visible for this block
                    if ((block.Faces & SideIndexToFaceBit(sideIdx)) == 0) continue;

                    FaceBakedData faceData = BakedFaces[blockType * 6 + sideIdx];
                    if (faceData.VertCount == 0) continue; // face not defined for this block type

                    int vertBase = blockType * 192 + sideIdx * 32 + rotIdx * 8;

                    float2 uv = sideIdx switch
                    {
                        0 => bakedBlock.UvFront,
                        1 => bakedBlock.UvRight,
                        2 => bakedBlock.UvBack,
                        3 => bakedBlock.UvLeft,
                        4 => bakedBlock.UvTop,
                        _ => bakedBlock.UvBottom
                    };

                    if (bakedBlock.IsLeaves && GenerateLeaves)
                    {
                        // Leaves into dedicated lists with full 0→1 UV coverage
                        for (int v = 0; v < faceData.VertCount; v++)
                            LeavesVerts.Add(VertPositions[vertBase + v] + blockPos);
                        LeavesUvs.Add(new float2(0f, 0f));
                        LeavesUvs.Add(new float2(1f, 0f));
                        LeavesUvs.Add(new float2(0f, 1f));
                        LeavesUvs.Add(new float2(1f, 1f));
                    }
                    else // Terrain — includes leaves when GenerateLeaves=false
                    {
                        int baseVert = TerrainVerts.Length;
                        for (int v = 0; v < faceData.VertCount; v++)
                        {
                            TerrainVerts.Add(VertPositions[vertBase + v] + blockPos);
                            TerrainUvs.Add(uv);
                        }
                        for (int t = 0; t < faceData.TriCount; t++)
                            TerrainTris.Add(baseVert + TriangleIndices[faceData.TriOffset + t]);
                    }
                }

                if (block.Position.Y + 1 > maxY) maxY = block.Position.Y + 1;
            }

            MaxY.Value = maxY;
        }

        // Maps BlockSide integer index to the corresponding BlockFaces bit.
        // BlockSide:  Front=0, Right=1, Back=2, Left=3, Top=4, Bottom=5
        // BlockFaces: Right=bit0, Left=bit1, Front=bit2, Back=bit3, Top=bit4, Bottom=bit5
        private static byte SideIndexToFaceBit(int sideIdx) => sideIdx switch
        {
            0 => 1 << 2, // Front
            1 => 1 << 0, // Right
            2 => 1 << 3, // Back
            3 => 1 << 1, // Left
            4 => 1 << 4, // Top
            _ => 1 << 5, // Bottom
        };
    }
}

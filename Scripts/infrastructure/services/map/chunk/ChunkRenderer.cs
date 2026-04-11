using System;
using System.Collections.Generic;
using System.Linq;
using infrastructure.factories;
using infrastructure.services.map;
using infrastructure.services.map.chunk;
using infrastructure.services.map.jobs;
using Sirenix.Utilities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkRenderer : PooledObject
{
    public const int ChunkWidth = 16;
    public const int ChunkWidthSquare = ChunkWidth * ChunkWidth;
    public const int ChunkHeight = 128;

    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private MeshCollider _meshCollider;

    private MeshBakery _meshBakery;
    private Material _waterMaterial;

    private WaterRenderer _waterRenderer;

    private static int[] _waterTriangles;
    private static bool _trianglesInited;

    public static Dictionary<BlockType, BlockData> BlockDatas;

    // --- Job state (allocated in ScheduleMeshJob, disposed in ApplyMeshResults) ---
    private JobHandle _pendingJobHandle;
    private NativeArray<RenderedBlockBlittable> _jobBlocks;
    private NativeList<float3> _jobTerrainVerts;
    private NativeList<float2> _jobTerrainUvs;
    private NativeList<int>    _jobTerrainTris;
    private NativeList<float3> _jobWaterVerts;
    private NativeList<float2> _jobWaterUvs;
    private NativeList<float3> _jobLeavesVerts;
    private NativeList<float2> _jobLeavesUvs;
    private NativeReference<int> _jobMaxY;
    private bool _jobAllocated;

    public ChunkData ChunkData { get; private set; }
    public Mesh ChunkMesh { get; private set; }
    public Mesh WaterMesh { get; private set; }

    [Inject]
    public void Construct(BlockTextureData blockTextureData, MeshBakery meshBakery)
    {
        _meshBakery = meshBakery;
        BlockDatas ??= blockTextureData.BlockDatas.ToDictionary(b => b.BlockType, b => b);
        InitTriangles();
    }

    public void SetData(ChunkData chunkData, Material worldMaterial, Material waterMaterial)
    {
        ChunkData = chunkData;
        _waterMaterial = waterMaterial;

        if (_meshRenderer.materials.IsNullOrEmpty())
        {
            _meshRenderer.SetMaterials(new List<Material> { worldMaterial });
        }
    }

    public void Generate()
    {
        ChunkMesh = new Mesh();
        _meshFilter.sharedMesh = ChunkMesh;
        ScheduleMeshJob().Complete();
        ApplyMeshResults();
    }

    public void RegenerateMesh()
    {
        ScheduleMeshJob().Complete();
        ApplyMeshResults();
    }

    public override void Release()
    {
        _waterRenderer?.Release();
        _waterRenderer = null;
        ChunkMesh = null;
        WaterMesh = null;
        base.Release();
    }

    public JobHandle ScheduleMeshJob()
    {
        if (ChunkMesh == null)
        {
            ChunkMesh = new Mesh();
            _meshFilter.sharedMesh = ChunkMesh;
        }

        int blockCount = ChunkData.Blocks.Count;
        _jobBlocks = new NativeArray<RenderedBlockBlittable>(blockCount, Allocator.TempJob);
        for (int i = 0; i < blockCount; i++)
        {
            var rb = ChunkData.Blocks[i];
            _jobBlocks[i] = new RenderedBlockBlittable
            {
                Position  = rb.Position,
                BlockType = (byte)rb.BlockType,
                Faces     = (byte)rb.Faces,
                Rotate    = rb.Rotate,
            };
        }

        _jobTerrainVerts = new NativeList<float3>(8192, Allocator.TempJob);
        _jobTerrainUvs   = new NativeList<float2>(8192, Allocator.TempJob);
        _jobTerrainTris  = new NativeList<int>(16384, Allocator.TempJob);
        _jobWaterVerts   = new NativeList<float3>(2048, Allocator.TempJob);
        _jobWaterUvs     = new NativeList<float2>(2048, Allocator.TempJob);
        _jobLeavesVerts  = new NativeList<float3>(4, Allocator.TempJob);
        _jobLeavesUvs    = new NativeList<float2>(4, Allocator.TempJob);
        _jobMaxY         = new NativeReference<int>(0, Allocator.TempJob);
        _jobAllocated    = true;

        var job = new ChunkMeshJob
        {
            Blocks          = _jobBlocks,
            BakedBlocks     = _meshBakery.BlockData,
            BakedFaces      = _meshBakery.FaceData,
            VertPositions   = _meshBakery.VertPositions,
            TriangleIndices = _meshBakery.TriangleIndices,
            GenerateLeaves  = false,
            TerrainVerts    = _jobTerrainVerts,
            TerrainUvs      = _jobTerrainUvs,
            TerrainTris     = _jobTerrainTris,
            WaterVerts      = _jobWaterVerts,
            WaterUvs        = _jobWaterUvs,
            LeavesVerts     = _jobLeavesVerts,
            LeavesUvs       = _jobLeavesUvs,
            MaxY            = _jobMaxY,
        };

        _pendingJobHandle = job.Schedule();
        return _pendingJobHandle;
    }

    public void ApplyMeshResults()
    {
        if (!_jobAllocated) return;
        _pendingJobHandle.Complete();
        _jobAllocated = false;

        // --- Terrain mesh ---
        ChunkMesh.triangles = Array.Empty<int>();
        ChunkMesh.SetVertices(_jobTerrainVerts.AsArray().Reinterpret<Vector3>());
        ChunkMesh.SetUVs(0, _jobTerrainUvs.AsArray().Reinterpret<Vector2>());
        ChunkMesh.subMeshCount = 1;
        ChunkMesh.SetIndices(_jobTerrainTris.AsArray(), MeshTopology.Triangles, 0, false);
        ChunkMesh.Optimize();
        ChunkMesh.RecalculateNormals();

        int maxY = _jobMaxY.Value;
        var boundsSize = new Vector3(ChunkWidth, maxY > 0 ? maxY : ChunkHeight, ChunkWidth);
        ChunkMesh.bounds = new Bounds(boundsSize / 2f, boundsSize);
        _meshCollider.sharedMesh = ChunkMesh;

        // --- Water mesh ---
        int waterVertCount = _jobWaterVerts.Length;
        if (waterVertCount > 0)
        {
            CreateWater();
            WaterMesh.triangles = Array.Empty<int>();
            WaterMesh.SetVertices(_jobWaterVerts.AsArray().Reinterpret<Vector3>());
            WaterMesh.SetUVs(0, _jobWaterUvs.AsArray().Reinterpret<Vector2>());
            WaterMesh.subMeshCount = 1;
            WaterMesh.SetTriangles(_waterTriangles, 0, waterVertCount * 6 / 4, 0, false);
            WaterMesh.Optimize();
            WaterMesh.RecalculateNormals();
            WaterMesh.bounds = new Bounds(boundsSize / 2f, boundsSize);
            _waterRenderer.WaterMeshFilter.sharedMesh = WaterMesh;
            _waterRenderer.WaterMeshCollider.sharedMesh = WaterMesh;
        }

        // --- Dispose TempJob allocations ---
        _jobBlocks.Dispose();
        _jobTerrainVerts.Dispose();
        _jobTerrainUvs.Dispose();
        _jobTerrainTris.Dispose();
        _jobWaterVerts.Dispose();
        _jobWaterUvs.Dispose();
        _jobLeavesVerts.Dispose();
        _jobLeavesUvs.Dispose();
        _jobMaxY.Dispose();
    }

    public static BlockData GetBlockData(BlockType blockType)
    {
        return BlockDatas.GetValueOrDefault(blockType);
    }

    private void CreateWater()
    {
        if (_waterRenderer != null) return;
        _waterRenderer = Pool.Get<WaterRenderer>();
        _waterRenderer.transform.SetParent(transform);
        _waterRenderer.transform.localPosition = Vector3.zero;
        WaterMesh = new Mesh();

        if (_waterRenderer.MeshRenderer.materials.IsNullOrEmpty())
        {
            _waterRenderer.MeshRenderer.SetMaterials(new List<Material> { _waterMaterial });
        }
    }

    private static void InitTriangles()
    {
        if (_trianglesInited) return;
        _trianglesInited = true;
        _waterTriangles = new int[65536 * 6 / 4];

        int vertexNumber = 4;
        for (int i = 0; i < _waterTriangles.Length; i += 6)
        {
            _waterTriangles[i]     = vertexNumber - 4;
            _waterTriangles[i + 1] = vertexNumber - 3;
            _waterTriangles[i + 2] = vertexNumber - 2;
            _waterTriangles[i + 3] = vertexNumber - 3;
            _waterTriangles[i + 4] = vertexNumber - 1;
            _waterTriangles[i + 5] = vertexNumber - 2;
            vertexNumber += 4;
        }
    }
}

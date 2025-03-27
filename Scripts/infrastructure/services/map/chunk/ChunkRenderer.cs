using System;
using System.Collections.Generic;
using infrastructure.services.map.chunk;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkRenderer : PooledObject
{
    public const int ChunkWidth = 16;
    public const int ChunkWidthSquare = ChunkWidth * ChunkWidth;
    public const int ChunkHeight = 128;

    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private MeshCollider _meshCollider;

    private readonly List<Vector3> _vertices = new List<Vector3>();
    private readonly List<Vector3> _waterVertices = new List<Vector3>();
    private readonly List<Vector2> _uvs = new List<Vector2>();
    private readonly List<Vector2> _waterUvs = new List<Vector2>();

    private BlockTextureData _blockTextureData;
    private IChunkFactory _chunkFactory;

    private WaterRenderer _waterRenderer;

    private static int[] _triangles;
    private static int[] _waterTriangles;
    private static bool _trianglesInited;
    
    public ChunkData ChunkData { get; private set; }
    public Mesh ChunkMesh { get; private set; }
    public Mesh WaterMesh { get; private set; }

    [Inject]
    public void Construct(BlockTextureData blockTextureData, IChunkFactory chunkFactory)
    {
        _blockTextureData = blockTextureData;
        _chunkFactory = chunkFactory;
        InitTriangles();
    }
    
    public void SetData(ChunkData chunkData)
    {
        ChunkData = chunkData;
    }

    public void Generate()
    {
        ChunkMesh = new Mesh();
        
        RegenerateMesh();

        _meshFilter.sharedMesh = ChunkMesh;
        if (_waterRenderer != null)
        {
            _waterRenderer.WaterMeshFilter.sharedMesh = WaterMesh;
        }
    }

    public override void Release()
    {
        _waterRenderer?.Release();
        _waterRenderer = null;
        ChunkMesh = null;
        WaterMesh = null;
        base.Release();
    }

    public void RegenerateMesh()
    {
        _vertices.Clear();
        _waterVertices.Clear();
        _uvs.Clear();
        _waterUvs.Clear();
        
        foreach (var renderedBlock in ChunkData.Blocks)
        {
            GenerateBlock(renderedBlock);
        }
        
        ChunkMesh.triangles = Array.Empty<int>();
        ChunkMesh.vertices = _vertices.ToArray();
        ChunkMesh.uv = _uvs.ToArray();
        ChunkMesh.subMeshCount = 1;
        ChunkMesh.SetTriangles(_triangles, 0, _vertices.Count  * 6 / 4, 0);
        ChunkMesh.Optimize();
        ChunkMesh.RecalculateNormals();
        ChunkMesh.RecalculateBounds();
        
        _meshCollider.sharedMesh = ChunkMesh;
        
        SetWaterMesh();
    }

    private void SetWaterMesh()
    {
        if (WaterMesh == null) return;
        
        WaterMesh.triangles = Array.Empty<int>();
        WaterMesh.vertices = _waterVertices.ToArray();
        WaterMesh.uv = _waterUvs.ToArray();
        WaterMesh.subMeshCount = 1;
        WaterMesh.SetTriangles(_waterTriangles, 0, _waterVertices.Count * 6 / 4, 0);
        WaterMesh.Optimize();
        
        WaterMesh.RecalculateNormals();
        WaterMesh.RecalculateBounds();
        _waterRenderer.WaterMeshCollider.sharedMesh = WaterMesh;
    }
    
    private void GenerateBlock(RenderedBlock renderedBlock)
    {
        if (renderedBlock.BlockType == BlockType.Water)
        {
            CreateWater();
            GenerateWaterTopSide(renderedBlock);
            return;
        }
        
        GenerateRightSide(renderedBlock);
        GenerateLeftSide(renderedBlock);
        GenerateFrontSide(renderedBlock);
        GenerateBackSide(renderedBlock);
        GenerateTopSide(renderedBlock);
        GenerateBottomSide(renderedBlock);
    }
    
    private void GenerateRightSide(RenderedBlock renderedBlock)
    {
        if (!renderedBlock.Right) return;
        
        _vertices.Add(new Vector3(1, 0, 0) + renderedBlock.Position);
        _vertices.Add(new Vector3(1, 1, 0) + renderedBlock.Position);
        _vertices.Add(new Vector3(1, 0, 1) + renderedBlock.Position);
        _vertices.Add(new Vector3(1, 1, 1) + renderedBlock.Position);
        
        AddUvs(renderedBlock.BlockType, BlockSide.Right, renderedBlock.Rotate);
    }

    private void GenerateLeftSide(RenderedBlock renderedBlock)
    {
        if (!renderedBlock.Left) return;

        _vertices.Add(new Vector3(0, 0, 0) + renderedBlock.Position);
        _vertices.Add(new Vector3(0, 0, 1) + renderedBlock.Position);
        _vertices.Add(new Vector3(0, 1, 0) + renderedBlock.Position);
        _vertices.Add(new Vector3(0, 1, 1) + renderedBlock.Position);
        
        AddUvs(renderedBlock.BlockType, BlockSide.Left, renderedBlock.Rotate);
    }
    
    private void GenerateFrontSide(RenderedBlock renderedBlock)
    {
        if (!renderedBlock.Front) return;

        _vertices.Add(new Vector3(0, 0, 1) + renderedBlock.Position);
        _vertices.Add(new Vector3(1, 0, 1) + renderedBlock.Position);
        _vertices.Add(new Vector3(0, 1, 1) + renderedBlock.Position);
        _vertices.Add(new Vector3(1, 1, 1) + renderedBlock.Position);
        
        AddUvs(renderedBlock.BlockType, BlockSide.Front, renderedBlock.Rotate);
    }
    
    private void GenerateBackSide(RenderedBlock renderedBlock)
    {
        if (!renderedBlock.Back) return;

        _vertices.Add(new Vector3(0, 0, 0) + renderedBlock.Position);
        _vertices.Add(new Vector3(0, 1, 0) + renderedBlock.Position);
        _vertices.Add(new Vector3(1, 0, 0) + renderedBlock.Position);
        _vertices.Add(new Vector3(1, 1, 0) + renderedBlock.Position);
        
        AddUvs(renderedBlock.BlockType, BlockSide.Back, renderedBlock.Rotate);
    }
    
    private void GenerateTopSide(RenderedBlock renderedBlock)
    {
        if (!renderedBlock.Top) return;

        _vertices.Add(new Vector3(0, 1, 0) + renderedBlock.Position);
        _vertices.Add(new Vector3(0, 1, 1) + renderedBlock.Position);
        _vertices.Add(new Vector3(1, 1, 0) + renderedBlock.Position);
        _vertices.Add(new Vector3(1, 1, 1) + renderedBlock.Position);
        
        AddUvs(renderedBlock.BlockType, BlockSide.Top, renderedBlock.Rotate);
    }
    
    private void GenerateBottomSide(RenderedBlock renderedBlock)
    {
        if (!renderedBlock.Bottom) return;

        _vertices.Add(new Vector3(0, 0, 0) + renderedBlock.Position);
        _vertices.Add(new Vector3(1, 0, 0) + renderedBlock.Position);
        _vertices.Add(new Vector3(0, 0, 1) + renderedBlock.Position);
        _vertices.Add(new Vector3(1, 0, 1) + renderedBlock.Position);
        
        AddUvs(renderedBlock.BlockType, BlockSide.Bottom, renderedBlock.Rotate);
    }
    
    private void GenerateWaterTopSide(RenderedBlock renderedBlock)
    {
        _waterVertices.Add(new Vector3(0, 0.75f, 0) + renderedBlock.Position);
        _waterVertices.Add(new Vector3(0, 0.75f, 1) + renderedBlock.Position);
        _waterVertices.Add(new Vector3(1, 0.75f, 0) + renderedBlock.Position);
        _waterVertices.Add(new Vector3(1, 0.75f, 1) + renderedBlock.Position);
        
        AddWaterUvs();
    }

    private void AddWaterUvs()
    {
        _waterUvs.Add(new Vector2(0, 0));
        _waterUvs.Add(new Vector2(1, 0));
        _waterUvs.Add(new Vector2(0, 1));
        _waterUvs.Add(new Vector2(1, 1));
    }
    
    private void AddUvs(BlockType blockType, BlockSide blockSide, int rotation = 0)
    {
        var blockTexture = _blockTextureData.GetBlockTexture(blockType);

        if (rotation != 0)
        {
            blockSide = (BlockSide)(((int)blockSide + rotation) % 4);
        }

        var index = blockSide switch
        {
            BlockSide.Left => blockTexture.LeftSideIndex,
            BlockSide.Right => blockTexture.RightSideIndex,
            BlockSide.Front => blockTexture.FrontSideIndex,
            BlockSide.Back => blockTexture.BackSideIndex,
            BlockSide.Top => blockTexture.TopSideIndex,
            BlockSide.Bottom => blockTexture.BottomSideIndex,
            _ => throw new ArgumentOutOfRangeException(nameof(blockSide), blockSide, null)
        };
        
        var uv = new Vector2(
            (index % _blockTextureData.CountBlocksInLine) * _blockTextureData.BlockSize / _blockTextureData.TextureSize, 
            (_blockTextureData.TextureSize - (index / _blockTextureData.CountBlocksInLine + 1) * _blockTextureData.BlockSize) / _blockTextureData.TextureSize
            );
    
        for (int i = 0; i < 4; i++)
        {
            _uvs.Add(uv);
        }
    }

    private void InitTriangles()
    {
        if (_trianglesInited) return;
        _trianglesInited = true;
        _triangles = new int[65536 * 6 / 4];
        _waterTriangles = new int[65536 * 6 / 4];

        int vertexNumber = 4;
        for (int i = 0; i < _triangles.Length; i+= 6)
        {
            _triangles[i] = vertexNumber - 4;
            _triangles[i + 1] = vertexNumber - 3;
            _triangles[i + 2] = vertexNumber - 2;
            
            _triangles[i + 3] = vertexNumber - 3;
            _triangles[i + 4] = vertexNumber - 1;
            _triangles[i + 5] = vertexNumber - 2;
            
            _waterTriangles[i] = vertexNumber - 4;
            _waterTriangles[i + 1] = vertexNumber - 3;
            _waterTriangles[i + 2] = vertexNumber - 2;
            
            _waterTriangles[i + 3] = vertexNumber - 3;
            _waterTriangles[i + 4] = vertexNumber - 1;
            _waterTriangles[i + 5] = vertexNumber - 2;
            vertexNumber += 4;
        }
    }

    private void CreateWater()
    {
        if (_waterRenderer != null) return;
        _waterRenderer = _chunkFactory.GetWater();
        _waterRenderer.transform.SetParent(transform);
        _waterRenderer.transform.localPosition = Vector3.zero;
        WaterMesh = new Mesh();
    }
}

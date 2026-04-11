using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace infrastructure.services.map
{
    public class DrawGraphics : MonoBehaviour
    {
        [SerializeField] private BlockData _blockData;
        [SerializeField] private Material _material; 
        private Mesh _mesh;
        private Matrix4x4[] _matrices = Array.Empty<Matrix4x4>();

        private IMapService _mapService;

        [Inject]
        public void Construct(IMapService mapService)
        {
            _mapService = mapService;

            _mapService.ChunksUpdated += CreateMatrix;
            CreateMesh();
        }

        private void OnDestroy()
        {
            _mapService.ChunksUpdated -= CreateMatrix;
        }        
        
        private void Update()
        {
            Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrices);    
        }

        private void CreateMatrix()
        {
            List<Matrix4x4> matrix = new List<Matrix4x4>();
            foreach (var chunkRenderer in _mapService.Chunks.Values)
            {
                foreach (var chunkDataBlock in chunkRenderer.ChunkData.Blocks)
                {
                    if (chunkDataBlock.BlockType != _blockData.BlockType) continue;
                    var pos = new Vector3(chunkDataBlock.Position.X + chunkRenderer.ChunkData.Position.X * ChunkRenderer.ChunkWidth,
                        chunkDataBlock.Position.Y, chunkDataBlock.Position.Z + chunkRenderer.ChunkData.Position.Y * ChunkRenderer.ChunkWidth);
                    matrix.Add(Matrix4x4.TRS(
                        pos,
                        Quaternion.identity,
                        Vector3.one
                    ));
                }
            }

            _matrices = matrix.ToArray();
        }
        
        private void CreateMesh()
        {
            var data = _blockData.VoxelMeshData;
            _mesh = new Mesh();
            var triangles = new List<int>();
            var verticies = new List<Vector3>();
            var uvs = new List<Vector2>();
            var trianglesCount = 0;
            for (var i = 0; i < data.Faces.Length; i++)
            {
                var faceMeshData = data.Faces[i];
                foreach (var vertData in faceMeshData.VertDatas)
                {
                    verticies.Add(vertData.Position);
                }

                if (faceMeshData.VertDatas.Length <= 0) continue;
                
                foreach (var triangle in faceMeshData.Triangles)
                {
                    triangles.Add(triangle + trianglesCount);
                }

                trianglesCount += faceMeshData.VertDatas.Length;
                
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));
            }

            _mesh.triangles = Array.Empty<int>();
            _mesh.vertices = verticies.ToArray();
            _mesh.uv = uvs.ToArray();
            _mesh.subMeshCount = 1;
            _mesh.SetTriangles(triangles, 0, triangles.Count, 0);
            _mesh.Optimize();
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
        }

    }
}
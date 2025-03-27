using UnityEngine;

namespace infrastructure.services.map.chunk
{
    public class WaterRenderer : PooledObject
    {
        [SerializeField] private MeshFilter _waterMeshFilter;
        [SerializeField] private MeshCollider _waterMeshCollider;

        public MeshFilter WaterMeshFilter => _waterMeshFilter;
        public MeshCollider WaterMeshCollider => _waterMeshCollider;
    }
}
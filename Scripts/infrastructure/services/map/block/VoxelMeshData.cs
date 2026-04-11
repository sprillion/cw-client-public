using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Voxel Mesh Data", menuName = "Data/VoxelMeshData")]
public class VoxelMeshData : ScriptableObject
{
    public VoxelMeshType VoxelMeshType;
    public FaceMeshData[] Faces;
}

[Serializable]
public class VertData
{
    public Vector3 Position;

    public VertData(Vector3 position)
    {
        Position = position;
    }

    public Vector3 GetRotatedPosition(Vector3 angle)
    {
        var center = new Vector3(0.5f, 0.5f, 0.5f);
        var direction = Position - center;
        direction = Quaternion.Euler(angle) * direction;
        return direction + center;
    }
}
    
[Serializable]
public class FaceMeshData
{
    public BlockSide BlockSide;
    public VertData[] VertDatas;
    public int[] Triangles;
}
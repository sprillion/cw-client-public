using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "Data/Map/BlockData")]
public class BlockData : ScriptableObject
{
    public BlockType BlockType;

    public VoxelMeshData VoxelMeshData;

    public int LeftSideIndex;
    public int RightSideIndex;
    public int FrontSideIndex;
    public int BackSideIndex;
    public int TopSideIndex;
    public int BottomSideIndex;

    public bool IsRotatableBlock;
    public bool DrawFull;
}
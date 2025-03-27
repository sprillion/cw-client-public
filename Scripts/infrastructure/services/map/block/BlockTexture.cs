using UnityEngine;

[CreateAssetMenu(fileName = "BlockTexture", menuName = "Data/Map/BlockTexture")]
public class BlockTexture : ScriptableObject
{
    public BlockType BlockType;

    public int LeftSideIndex;
    public int RightSideIndex;
    public int FrontSideIndex;
    public int BackSideIndex;
    public int TopSideIndex;
    public int BottomSideIndex;

    public bool IsRotatableBlock;
}
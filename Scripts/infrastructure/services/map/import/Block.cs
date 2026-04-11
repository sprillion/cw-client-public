using System;

[Serializable]
public class Block
{
    public BlockType BlockType;
    public Vector3Short Position; //Position
    public int Value; //Rendered sides + rotation
    public short Rotation;
}
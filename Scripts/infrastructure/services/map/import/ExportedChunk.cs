using System;
using System.Collections.Generic;

[Serializable]
public class ExportedChunk
{
    public Vector2Short Position; // Chunk position
    public List<Block> Blocks;   //Chunk blocks
}
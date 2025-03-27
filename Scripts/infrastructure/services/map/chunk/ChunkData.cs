using System.Collections.Generic;

namespace infrastructure.services.map.chunk
{
    public class ChunkData
    {
        public Vector2Short Position;
        public List<RenderedBlock> Blocks;
    }
}
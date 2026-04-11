using Unity.Mathematics;

namespace infrastructure.services.map.jobs
{
    // Per block-type baked data (indexed 0-255 by (byte)BlockType)
    public struct BlockBakedData
    {
        public float2 UvFront, UvRight, UvBack, UvLeft, UvTop, UvBottom;
        public bool IsRotatable;
        public bool IsWater;
        public bool IsLeaves;
    }

    // Per (blockType * 6 + sideIndex) face baked data — 256 * 6 = 1536 entries
    // VertPositions layout: [blockType * 192 + sideIdx * 32 + rotIdx * 8 + vertIdx]
    //   where 192 = 6 sides * 4 rotations * 8 max-verts-per-face
    // TriangleIndices layout: [blockType * 72 + sideIdx * 12 + triIdx]
    public struct FaceBakedData
    {
        public int VertCount;   // 0 = face not defined for this block type
        public int TriCount;    // number of triangle indices (typically 6)
        public int TriOffset;   // base index into TriangleIndices array
    }

    // Blittable INPUT struct — converted from RenderedBlock before scheduling the job.
    // Faces uses the same bit layout as BlockFaces:
    //   bit 0 = Right, bit 1 = Left, bit 2 = Front, bit 3 = Back, bit 4 = Top, bit 5 = Bottom
    public struct RenderedBlockBlittable
    {
        public Vector3Short Position;
        public byte BlockType;
        public byte Faces;
        public short Rotate;
    }
}

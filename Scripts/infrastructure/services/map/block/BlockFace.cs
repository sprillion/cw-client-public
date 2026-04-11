using System;

namespace infrastructure.services.map
{
    [Flags]
    public enum BlockFaces : byte
    {
        None   = 0,
        Right  = 1 << 0,
        Left   = 1 << 1,
        Front  = 1 << 2,
        Back   = 1 << 3,
        Top    = 1 << 4,
        Bottom = 1 << 5
    }
}
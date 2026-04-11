using System.Collections.Generic;
using character;
using network;

namespace infrastructure.services.mobs
{
    public interface IMobService : IReceiver
    {
        IReadOnlyDictionary<ushort, Mob> Mobs { get; }
    }
}
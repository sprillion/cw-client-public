using environment.chests;
using infrastructure.services.npc;

namespace infrastructure.factories.environment
{
    public interface IEnvironmentFactory
    {
        ChestFromMob CreateChestFromMob();
        Npc CreateNpc(NpcType npcType);
    }
}
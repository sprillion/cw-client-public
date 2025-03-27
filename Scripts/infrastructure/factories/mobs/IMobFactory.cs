using infrastructure.services.mobs;

namespace factories.mobs
{
    public interface IMobFactory
    {
        Mob GetNewMob(MobType mobType);
    }
}
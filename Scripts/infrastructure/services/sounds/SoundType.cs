namespace infrastructure.services.sounds
{
    public enum SoundType
    {
        None,

        // UI
        ButtonClick,
        ButtonHover,

        // Character
        Jump,
        Land,

        // Footsteps
        FootstepGrass,
        FootstepDirt,
        FootstepStone,
        FootstepWood,
        FootstepSand,
        FootstepSnow,
        FootstepMetal,

        // Combat
        Attack,
        HitReceived,
        Death,

        // World
        BlockPlace,
        ItemPickup,

        // Music
        GameMusic,
        MenuMusic,
    }
}

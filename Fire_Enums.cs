namespace WildFire
{
    public enum FireType
    {
        Unknown,
        Campfire,
        Stove,
        Fireplace,
        Torch,
        Other
    }

    public enum FireStage
    {
        Unknown,
        Embers,
        Small,
        Medium,
        Large,
        FullBurn,
        Accelerant,
        FlareupSmall,
        FlareupLarge,
        Sparks,  // Added for fuel addition spark effects
        Other
    }
}
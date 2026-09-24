using System;

namespace Economy
{
    public enum ResourceType
    {
        DarkMatter = 0,
        GlowingPlasma = 1,
        KordiumCrystals = 2
    }

    public static class ResourceTypes
    {
        public static readonly ResourceType[] All = (ResourceType[])Enum.GetValues(typeof(ResourceType));
        public static readonly int Count = All.Length;
    }
}

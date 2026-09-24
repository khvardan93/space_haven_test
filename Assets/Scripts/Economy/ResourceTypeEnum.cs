using System;

namespace Economy
{
    public enum ResourceTypeEnum
    {
        DarkMatter = 0,
        GlowingPlasma = 1,
        KordiumCrystals = 2
    }

    public static class ResourceTypes
    {
        public static readonly ResourceTypeEnum[] All = (ResourceTypeEnum[])Enum.GetValues(typeof(ResourceTypeEnum));
        public static readonly int Count = All.Length;
    }
}

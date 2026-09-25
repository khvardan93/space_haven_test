using System;

namespace Economy
{
    public static class ResourceTypes
    {
        public static readonly ResourceTypeEnum[] All = (ResourceTypeEnum[])Enum.GetValues(typeof(ResourceTypeEnum));
        public static readonly int Count = All.Length;
    }
}

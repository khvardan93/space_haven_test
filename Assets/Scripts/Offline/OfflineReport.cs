using System;
using Economy;

namespace Offline
{
    public sealed class OfflineReport
    {
        public static readonly OfflineReport Empty = new(TimeSpan.Zero, false, new double[ResourceTypes.Count]);

        public TimeSpan Elapsed { get; private set; }
        public bool WasCapped { get; private set; }

        public double[] NetChange { get; private set; }

        public bool HasGains
        {
            get
            {
                foreach (var value in NetChange)
                {
                    if (value > 0) return true;
                }
                return false;
            }
        }

        public OfflineReport(TimeSpan elapsed, bool wasCapped, double[] netChange)
        {
            Elapsed = elapsed;
            WasCapped = wasCapped;
            NetChange = netChange;
        }

        public double Get(ResourceTypeEnum typeEnum)
        {
            return NetChange[(int)typeEnum];
        }
    }
}

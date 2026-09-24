using System;
using Economy;
using Simulation;

namespace Offline
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }

    public sealed class SystemTimeProvider : ITimeProvider
    {
        public DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }
    }

    public sealed class OfflineReport
    {
        public static readonly OfflineReport Empty = new OfflineReport(TimeSpan.Zero, false, new double[ResourceTypes.Count]);

        public TimeSpan Elapsed { get; private set; }
        public bool WasCapped { get; private set; }

        /// <summary>Net change per resource, indexed by (int)ResourceType. Can be negative for consumed inputs.</summary>
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

        public double Get(ResourceType type)
        {
            return NetChange[(int)type];
        }
    }

    /// <summary>
    /// Catches up time spent away by running the normal simulation in 1s steps.
    /// Chains, starvation, laser regen and boost expiry all behave exactly as in live play.
    /// </summary>
    public sealed class OfflineEarnings
    {
        private readonly ITimeProvider _time;
        private readonly TimeSpan _cap;
        private readonly double _step;

        public OfflineEarnings(ITimeProvider time, TimeSpan cap, double stepSeconds = 1.0)
        {
            if (time == null) throw new ArgumentNullException(nameof(time));
            if (cap <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(cap));
            if (stepSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(stepSeconds));

            _time = time;
            _cap = cap;
            _step = stepSeconds;
        }

        /// <summary>
        /// Call after loading a save and before views subscribe to events,
        /// so thousands of catch-up cycles do not spawn floating text.
        /// </summary>
        public OfflineReport Apply(DateTime lastSaveUtc, GameSimulation simulation, GameEconomy economy)
        {
            if (simulation == null) throw new ArgumentNullException(nameof(simulation));
            if (economy == null) throw new ArgumentNullException(nameof(economy));

            TimeSpan elapsed = _time.UtcNow - lastSaveUtc;

            // Clock moved backwards (manual change or timezone bug): grant nothing.
            if (elapsed <= TimeSpan.Zero)
                return OfflineReport.Empty;

            bool capped = elapsed > _cap;
            if (capped)
                elapsed = _cap;

            double[] before = economy.Snapshot();
            simulation.Advance(elapsed.TotalSeconds, _step);
            double[] after = economy.Snapshot();

            var net = new double[ResourceTypes.Count];
            for (int i = 0; i < net.Length; i++)
                net[i] = after[i] - before[i];

            return new OfflineReport(elapsed, capped, net);
        }
    }
}

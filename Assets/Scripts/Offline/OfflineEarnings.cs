using System;
using Economy;
using Simulation;

namespace Offline
{
    public sealed class OfflineEarnings
    {
        private readonly ITimeProvider _time;
        private readonly TimeSpan _cap;
        private readonly double _step;

        public OfflineEarnings(ITimeProvider time, TimeSpan cap, double stepSeconds = 1.0)
        {
            if (cap <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(cap));
            if (stepSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(stepSeconds));

            _time = time ?? throw new ArgumentNullException(nameof(time));
            _cap = cap;
            _step = stepSeconds;
        }

        public OfflineReport Apply(DateTime lastSaveUtc, GameSimulation simulation, GameEconomy economy)
        {
            if (simulation == null) throw new ArgumentNullException(nameof(simulation));
            if (economy == null) throw new ArgumentNullException(nameof(economy));

            var elapsed = _time.UtcNow - lastSaveUtc;

            if (elapsed <= TimeSpan.Zero)
                return OfflineReport.Empty;

            var capped = elapsed > _cap;
            if (capped)
                elapsed = _cap;

            var before = economy.Snapshot();
            simulation.Advance(elapsed.TotalSeconds, _step);
            var after = economy.Snapshot();

            var net = new double[ResourceTypes.Count];
            for (var i = 0; i < net.Length; i++)
                net[i] = after[i] - before[i];

            return new OfflineReport(elapsed, capped, net);
        }
    }
}

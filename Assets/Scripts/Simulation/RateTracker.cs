using System;
using Economy;
using Prison;

namespace Simulation
{
    /// <summary>
    /// Per-second production and consumption rates for the economy panel.
    ///
    /// Each step it computes the instantaneous rate from room state (level, boost, starvation)
    /// and smooths it with an exponential moving average. Why not count produced resources over a window:
    /// with 2s and 5s cycles a 5s window aliases (two cells show 1.6 or 2.4 instead of 2.0).
    /// Why smooth at all: a refinery that is starved part of the time flickers between full rate and 0;
    /// the average shows its real throughput. Rates are per simulated second.
    /// </summary>
    public sealed class RateTracker : IDisposable
    {
        private const double RefreshInterval = 0.5;

        private readonly PrisonBlock _prison;
        private readonly GameSimulation _simulation;
        private readonly double _smoothingSeconds;
        private readonly double[] _production = new double[ResourceTypes.Count];
        private readonly double[] _consumption = new double[ResourceTypes.Count];
        private readonly double[] _instantProduction = new double[ResourceTypes.Count];
        private readonly double[] _instantConsumption = new double[ResourceTypes.Count];
        private double _sinceRefresh;
        private bool _hasSample;

        /// <summary>Raised twice per simulated second, a good moment for the panel to refresh rate labels.</summary>
        public event Action Updated;

        public RateTracker(PrisonBlock prison, GameSimulation simulation, double smoothingSeconds = 2.0)
        {
            if (prison == null) throw new ArgumentNullException(nameof(prison));
            if (simulation == null) throw new ArgumentNullException(nameof(simulation));
            if (smoothingSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(smoothingSeconds));

            _prison = prison;
            _simulation = simulation;
            _smoothingSeconds = smoothingSeconds;
            _simulation.Stepped += OnStepped;
        }

        public double GetProduction(ResourceType type)
        {
            return _production[(int)type];
        }

        public double GetConsumption(ResourceType type)
        {
            return _consumption[(int)type];
        }

        public double GetNet(ResourceType type)
        {
            return GetProduction(type) - GetConsumption(type);
        }

        public void Dispose()
        {
            _simulation.Stepped -= OnStepped;
        }

        private void OnStepped(double dt)
        {
            ComputeInstant();

            // First sample: jump straight to the value instead of easing in from 0.
            var alpha = _hasSample ? 1 - Math.Exp(-dt / _smoothingSeconds) : 1;
            _hasSample = true;

            for (var i = 0; i < ResourceTypes.Count; i++)
            {
                _production[i] += (_instantProduction[i] - _production[i]) * alpha;
                _consumption[i] += (_instantConsumption[i] - _consumption[i]) * alpha;
            }

            _sinceRefresh += dt;
            if (_sinceRefresh >= RefreshInterval)
            {
                _sinceRefresh = 0;
                var handler = Updated;
                if (handler != null) handler();
            }
        }

        private void ComputeInstant()
        {
            Array.Clear(_instantProduction, 0, _instantProduction.Length);
            Array.Clear(_instantConsumption, 0, _instantConsumption.Length);

            var rooms = _prison.Rooms;
            for (var r = 0; r < rooms.Count; r++)
            {
                var room = rooms[r];
                if (room.IsStarved)
                    continue;

                var cyclesPerSecond = (room.IsBoosted ? room.BoostMultiplier : 1) / room.Spec.CycleTime;

                var outputs = room.CurrentOutputs;
                for (var i = 0; i < outputs.Count; i++)
                    _instantProduction[(int)outputs[i].Type] += outputs[i].Amount * cyclesPerSecond;

                var inputs = room.Spec.Inputs;
                for (var i = 0; i < inputs.Count; i++)
                    _instantConsumption[(int)inputs[i].Type] += inputs[i].Amount * cyclesPerSecond;
            }
        }
    }
}

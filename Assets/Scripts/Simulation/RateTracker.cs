using System;
using Economy;
using Prison;

namespace Simulation
{
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

        public event Action Updated;

        public RateTracker(PrisonBlock prison, GameSimulation simulation, double smoothingSeconds = 2.0)
        {
            if (smoothingSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(smoothingSeconds));

            _prison = prison ?? throw new ArgumentNullException(nameof(prison));
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            _smoothingSeconds = smoothingSeconds;
            _simulation.Stepped += OnStepped;
        }

        public double GetProduction(ResourceTypeEnum typeEnum)
        {
            return _production[(int)typeEnum];
        }

        public double GetConsumption(ResourceTypeEnum typeEnum)
        {
            return _consumption[(int)typeEnum];
        }

        public double GetNet(ResourceTypeEnum typeEnum)
        {
            return GetProduction(typeEnum) - GetConsumption(typeEnum);
        }

        public void Dispose()
        {
            _simulation.Stepped -= OnStepped;
        }

        private void OnStepped(double dt)
        {
            ComputeInstant();

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
                Updated?.Invoke();
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
                    _instantProduction[(int)outputs[i].TypeEnum] += outputs[i].Amount * cyclesPerSecond;

                var inputs = room.Spec.Inputs;
                for (var i = 0; i < inputs.Count; i++)
                    _instantConsumption[(int)inputs[i].TypeEnum] += inputs[i].Amount * cyclesPerSecond;
            }
        }
    }
}

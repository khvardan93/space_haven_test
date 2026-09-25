using System;
using Economy;
using Laser;
using Prison;

namespace Simulation
{
    public sealed class GameSimulation
    {
        public const double FixedStep = 0.1;

        private const int MaxStepsPerUpdate = 10;

        private const double TimeEpsilon = 1e-9;

        private readonly GameEconomy _economy;
        private readonly LaserEnergy _laser;
        private readonly PrisonBlock _prison;
        private double _accumulator;
        private float _timeScale = 1f;

        public float TimeScale
        {
            get => _timeScale; 
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _timeScale = value;
            }
        }

        public double SimulatedTime { get; private set; }

        public event Action<double> Stepped;

        public GameSimulation(GameEconomy economy, LaserEnergy laser, PrisonBlock prison)
        {
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _laser = laser ?? throw new ArgumentNullException(nameof(laser));
            _prison = prison ?? throw new ArgumentNullException(nameof(prison));
        }

        public void Update(double realDeltaTime)
        {
            if (realDeltaTime <= 0 || _timeScale <= 0)
                return;

            _accumulator += realDeltaTime * _timeScale;

            var steps = 0;
            while (_accumulator >= FixedStep - TimeEpsilon && steps < MaxStepsPerUpdate)
            {
                Step(FixedStep);
                _accumulator = Math.Max(0, _accumulator - FixedStep);
                steps++;
            }

            if (steps == MaxStepsPerUpdate && _accumulator > FixedStep)
                _accumulator = FixedStep;
        }

        public void Advance(double seconds, double step)
        {
            if (seconds <= 0)
                return;
            if (step <= 0)
                throw new ArgumentOutOfRangeException(nameof(step));

            var remaining = seconds;
            while (remaining >= step - TimeEpsilon)
            {
                Step(step);
                remaining = Math.Max(0, remaining - step);
            }
            if (remaining > TimeEpsilon)
                Step(remaining);
        }

        private void Step(double dt)
        {
            _laser.Tick(dt);

            var rooms = _prison.Rooms;
            for (var i = 0; i < rooms.Count; i++)
                rooms[i].Tick(dt, _economy);

            SimulatedTime += dt;

            Stepped?.Invoke(dt);
        }
    }
}

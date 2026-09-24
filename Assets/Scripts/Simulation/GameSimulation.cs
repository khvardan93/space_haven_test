using System;
using Economy;
using Laser;
using Prison;

namespace Simulation
{
    /// <summary>
    /// Fixed-step game loop. Live play and offline catch-up both go through Step(),
    /// so there is exactly one code path for production.
    /// </summary>
    public sealed class GameSimulation
    {
        public const double FixedStep = 0.1;

        // Caps catch-up work per frame (1s of game time at x1) to avoid a spiral of death after a hitch.
        private const int MaxStepsPerUpdate = 10;

        // Tolerance for accumulated floating point error in time sums.
        private const double TimeEpsilon = 1e-9;

        private readonly GameEconomy _economy;
        private readonly LaserEnergy _laser;
        private readonly PrisonBlock _prison;
        private double _accumulator;
        private float _timeScale = 1f;

        /// <summary>1 for normal speed, 5 for the demo fast-forward toggle.</summary>
        public float TimeScale
        {
            get { return _timeScale; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _timeScale = value;
            }
        }

        /// <summary>Total simulated seconds since this object was created.</summary>
        public double SimulatedTime { get; private set; }

        /// <summary>Raised after each simulation step with its duration. RateTracker uses it as its clock.</summary>
        public event Action<double> Stepped;

        public GameSimulation(GameEconomy economy, LaserEnergy laser, PrisonBlock prison)
        {
            if (economy == null) throw new ArgumentNullException(nameof(economy));
            if (laser == null) throw new ArgumentNullException(nameof(laser));
            if (prison == null) throw new ArgumentNullException(nameof(prison));

            _economy = economy;
            _laser = laser;
            _prison = prison;
        }

        /// <summary>Call once per frame with unscaled real delta time.</summary>
        public void Update(double realDeltaTime)
        {
            if (realDeltaTime <= 0 || _timeScale <= 0)
                return;

            _accumulator += realDeltaTime * _timeScale;

            int steps = 0;
            while (_accumulator >= FixedStep - TimeEpsilon && steps < MaxStepsPerUpdate)
            {
                Step(FixedStep);
                _accumulator = Math.Max(0, _accumulator - FixedStep);
                steps++;
            }

            // Drop backlog we could not process this frame instead of piling it up.
            if (steps == MaxStepsPerUpdate && _accumulator > FixedStep)
                _accumulator = FixedStep;
        }

        /// <summary>Runs a block of time immediately, e.g. offline catch-up. Uses larger steps for speed.</summary>
        public void Advance(double seconds, double step)
        {
            if (seconds <= 0)
                return;
            if (step <= 0)
                throw new ArgumentOutOfRangeException(nameof(step));

            double remaining = seconds;
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
            for (int i = 0; i < rooms.Count; i++)
                rooms[i].Tick(dt, _economy);

            SimulatedTime += dt;

            var handler = Stepped;
            if (handler != null) handler(dt);
        }
    }
}

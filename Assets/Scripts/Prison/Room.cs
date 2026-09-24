using System;
using System.Collections.Generic;
using Configs;
using Economy;

namespace Prison
{
    /// <summary>
    /// A built room in a prison slot. Runs production cycles: inputs are consumed and outputs produced
    /// at the end of each cycle. Without inputs the room waits with a full bar (starved) until they arrive.
    /// </summary>
    public sealed class Room
    {
        // Safety net against a runaway loop from a huge dt or a tiny cycle time.
        private const int MaxCyclesPerTick = 10000;

        // Summing 0.1 twenty times gives 1.9999999, not 2. Without a tolerance cycles fire one step late.
        private const double TimeEpsilon = 1e-9;

        private Resource[] _currentOutputs;

        public RoomConfigs Spec { get; private set; }
        public int Level { get; private set; }
        public double Progress { get; private set; }
        public bool IsStarved { get; private set; }
        public double BoostRemaining { get; private set; }
        public double BoostMultiplier { get; private set; }

        public bool IsBoosted
        {
            get { return BoostRemaining > 0; }
        }

        public bool IsMaxLevel
        {
            get { return Level >= Spec.MaxLevel; }
        }

        public float Progress01
        {
            get { return (float)Math.Min(1.0, Progress / Spec.CycleTime); }
        }

        public IReadOnlyList<Resource> CurrentOutputs
        {
            get { return _currentOutputs; }
        }

        /// <summary>Output per second at the current level, ignoring boost and starvation.</summary>
        public double GetBaseRate(ResourceType type)
        {
            double total = 0;
            foreach (var output in _currentOutputs)
            {
                if (output.Type == type)
                    total += output.Amount;
            }
            return total / Spec.CycleTime;
        }

        /// <summary>Raised after outputs are added to the economy. Arguments are the room and what it produced.</summary>
        public event Action<Room, IReadOnlyList<Resource>> Produced;
        /// <summary>Raised after inputs are taken from the economy.</summary>
        public event Action<Room, IReadOnlyList<Resource>> Consumed;
        public event Action<Room> StateChanged;

        public Room(RoomConfigs spec, int level = 1)
        {
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            Spec = spec;
            BoostMultiplier = 1;
            SetLevel(level);
        }

        public void Tick(double dt, GameEconomy economy)
        {
            if (dt <= 0)
                return;

            Progress += EffectiveTime(dt);

            int cycles = 0;
            while (Progress >= Spec.CycleTime - TimeEpsilon && cycles < MaxCyclesPerTick)
            {
                if (Spec.Inputs.Count > 0)
                {
                    if (!economy.TrySpend(Spec.Inputs))
                    {
                        // Hold the bar at full and fire as soon as inputs arrive.
                        Progress = Spec.CycleTime;
                        SetStarved(true);
                        return;
                    }
                    RaiseConsumed(Spec.Inputs);
                }

                SetStarved(false);
                economy.Add(_currentOutputs);
                RaiseProduced(_currentOutputs);

                Progress = Math.Max(0, Progress - Spec.CycleTime);
                cycles++;
            }

            if (cycles >= MaxCyclesPerTick)
                Progress = 0;
        }

        /// <summary>Refreshes the boost to the given duration. Boosts do not stack.</summary>
        public void ApplyBoost(double duration, double multiplier)
        {
            if (duration <= 0) throw new ArgumentOutOfRangeException(nameof(duration));
            if (multiplier < 1) throw new ArgumentOutOfRangeException(nameof(multiplier));

            BoostRemaining = duration;
            BoostMultiplier = multiplier;
            RaiseStateChanged();
        }

        internal void SetLevel(int level)
        {
            Level = Math.Max(1, Math.Min(level, Spec.MaxLevel));
            _currentOutputs = Resource.Scale(Spec.Outputs, Spec.GetOutputMultiplier(Level));
            RaiseStateChanged();
        }

        internal void RestoreState(double progress, double boostRemaining, double boostMultiplier)
        {
            Progress = Math.Max(0, Math.Min(progress, Spec.CycleTime));
            BoostRemaining = Math.Max(0, boostRemaining);
            BoostMultiplier = Math.Max(1, boostMultiplier);
        }

        /// <summary>
        /// Converts real time into production time. If the boost ends inside this tick,
        /// only the boosted part is multiplied, so offline steps of 1s stay exact.
        /// </summary>
        private double EffectiveTime(double dt)
        {
            if (!IsBoosted)
                return dt;

            double boosted = Math.Min(dt, BoostRemaining);
            BoostRemaining -= boosted;

            if (BoostRemaining <= 0)
            {
                BoostRemaining = 0;
                RaiseStateChanged();
            }

            return boosted * BoostMultiplier + (dt - boosted);
        }

        private void SetStarved(bool value)
        {
            if (IsStarved == value)
                return;

            IsStarved = value;
            RaiseStateChanged();
        }

        private void RaiseProduced(IReadOnlyList<Resource> amounts)
        {
            var handler = Produced;
            if (handler != null) handler(this, amounts);
        }

        private void RaiseConsumed(IReadOnlyList<Resource> amounts)
        {
            var handler = Consumed;
            if (handler != null) handler(this, amounts);
        }

        private void RaiseStateChanged()
        {
            var handler = StateChanged;
            if (handler != null) handler(this);
        }
    }
}

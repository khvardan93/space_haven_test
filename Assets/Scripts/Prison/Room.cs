using System;
using System.Collections.Generic;
using Configs;
using Economy;

namespace Prison
{
    
    public sealed class Room
    {
        
        private const int MaxCyclesPerTick = 10000;

        private const double TimeEpsilon = 1e-9;

        private Resource[] _currentOutputs;

        public RoomConfigs Spec { get; private set; }
        public int Level { get; private set; }
        public double Progress { get; private set; }
        public bool IsStarved { get; private set; }
        public double BoostRemaining { get; private set; }
        public double BoostMultiplier { get; private set; }

        public bool IsBoosted => BoostRemaining > 0; 

        public bool IsMaxLevel => Level >= Spec.MaxLevel; 

        public float Progress01 => (float)Math.Min(1.0, Progress / Spec.CycleTime); 

        public IReadOnlyList<Resource> CurrentOutputs => _currentOutputs; 

        public double GetBaseRate(ResourceTypeEnum typeEnum)
        {
            var total = 0d;
            foreach (var output in _currentOutputs)
            {
                if (output.TypeEnum == typeEnum)
                    total += output.Amount;
            }
            return total / Spec.CycleTime;
        }

        public event Action<Room, IReadOnlyList<Resource>> Produced;
        
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

            var cycles = 0;
            while (Progress >= Spec.CycleTime - TimeEpsilon && cycles < MaxCyclesPerTick)
            {
                if (Spec.Inputs.Count > 0)
                {
                    if (!economy.TrySpend(Spec.Inputs))
                    {
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

        private double EffectiveTime(double dt)
        {
            if (!IsBoosted)
                return dt;

            var boosted = Math.Min(dt, BoostRemaining);
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
            Produced?.Invoke(this, amounts);
        }

        private void RaiseConsumed(IReadOnlyList<Resource> amounts)
        {
            Consumed?.Invoke(this, amounts);
        }

        private void RaiseStateChanged()
        {
            StateChanged?.Invoke(this);
        }
    }
}

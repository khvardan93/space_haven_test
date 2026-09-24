using System;
using Configs;

namespace Laser
{
    /// <summary>
    /// Regenerating action points. Building and upgrading cost Laser Energy (action limiting),
    /// and it can be spent to boost a room's speed. Regen pauses while the pool is full.
    /// </summary>
    public sealed class LaserEnergy
    {
        private readonly LaserSettings _spec;
        private double _timer;

        public int Current { get; private set; }

        public int Max => _spec.Max; 

        public bool IsFull => Current >= Max; 

        /// <summary>Seconds until the next point regenerates, 0 when full. Used by the HUD timer.</summary>
        public double SecondsToNext => IsFull ? 0 : _spec.RegenSeconds - _timer; 

        /// <summary>Elapsed time toward the next point, stored in saves.</summary>
        public double RegenTimer => _timer; 

        public event Action<int> Changed;

        public LaserEnergy(LaserSettings spec)
        {
            if (spec.Max < 1) throw new ArgumentException("Laser max must be >= 1.");
            if (spec.RegenSeconds <= 0) throw new ArgumentException("Laser regen time must be > 0.");

            _spec = spec;
            Current = Clamp(spec.LaserStart);
        }

        public void Tick(double dt)
        {
            if (dt <= 0)
                return;

            if (IsFull)
            {
                _timer = 0;
                return;
            }

            _timer += dt;
            var gained = 0;
            while (_timer >= _spec.RegenSeconds - 1e-9 && !IsFull)
            {
                _timer = Math.Max(0, _timer - _spec.RegenSeconds);
                Current++;
                gained++;
            }

            if (IsFull)
                _timer = 0;
            if (gained > 0)
                RaiseChanged();
        }

        public bool CanSpend(int amount)
        {
            return amount <= 0 || Current >= amount;
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0) return true;
            if (Current < amount) return false;

            Current -= amount;
            RaiseChanged();
            return true;
        }

        /// <summary>Adds energy up to Max. Hook for a future rewarded-ad refill.</summary>
        public void Restore(int amount)
        {
            if (amount <= 0 || IsFull)
                return;

            Current = Clamp(Current + amount);
            if (IsFull)
                _timer = 0;
            RaiseChanged();
        }

        internal void RestoreState(int current, double timer)
        {
            Current = Clamp(current);
            _timer = IsFull ? 0 : Math.Max(0, Math.Min(timer, _spec.RegenSeconds));
            RaiseChanged();
        }

        private int Clamp(int value)
        {
            return Math.Max(0, Math.Min(value, _spec.Max));
        }

        private void RaiseChanged()
        {
            Changed?.Invoke(Current);
        }
    }
}

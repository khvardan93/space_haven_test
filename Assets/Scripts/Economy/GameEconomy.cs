using System;
using System.Collections.Generic;
using Configs;

namespace Economy
{
    /// <summary>
    /// Holds resource balances. All spending is atomic: a multi-resource cost is either paid in full or not at all.
    /// </summary>
    public sealed class GameEconomy
    {
        // Tolerance for floating point drift, so 9.9999999 counts as affording 10.
        private const double Epsilon = 1e-9;

        private readonly double[] _balances = new double[ResourceTypes.Count];
        private readonly double[] _scratch = new double[ResourceTypes.Count];

        public event Action<ResourceType, double> Changed;

        public double Get(ResourceType type)
        {
            return _balances[(int)type];
        }

        public void Add(ResourceType type, double amount)
        {
            if (amount < 0 || double.IsNaN(amount) || double.IsInfinity(amount))
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Use TrySpend to remove resources.");
            if (amount == 0)
                return;

            _balances[(int)type] += amount;
            RaiseChanged(type);
        }

        public void Add(IReadOnlyList<ResourceAmount> amounts)
        {
            for (var i = 0; i < amounts.Count; i++)
                Add(amounts[i].Resource.Type, amounts[i].Amount);
        }
        
        public void Add(IReadOnlyList<Resource> amounts)
        {
            for (var i = 0; i < amounts.Count; i++)
                Add(amounts[i].Type, amounts[i].Amount);
        }

        public bool CanAfford(IReadOnlyList<Resource> cost)
        {
            if (cost == null || cost.Count == 0)
                return true;

            SumByType(cost);
            for (var i = 0; i < _scratch.Length; i++)
            {
                if (_scratch[i] > _balances[i] + Epsilon)
                    return false;
            }
            return true;
        }

        public bool TrySpend(IReadOnlyList<Resource> cost)
        {
            if (!CanAfford(cost))
                return false;
            if (cost == null || cost.Count == 0)
                return true;

            // _scratch still holds the per-type totals computed by CanAfford.
            for (var i = 0; i < _scratch.Length; i++)
            {
                if (_scratch[i] <= 0)
                    continue;

                _balances[i] = Math.Max(0, _balances[i] - _scratch[i]);
                RaiseChanged((ResourceType)i);
            }
            return true;
        }

        public double[] Snapshot()
        {
            return (double[])_balances.Clone();
        }

        /// <summary>Replaces all balances, used when loading a save. Missing entries become 0.</summary>
        public void SetAll(IReadOnlyList<double> balances)
        {
            for (var i = 0; i < _balances.Length; i++)
            {
                var value = balances != null && i < balances.Count ? balances[i] : 0;
                _balances[i] = double.IsNaN(value) || value < 0 ? 0 : value;
                RaiseChanged((ResourceType)i);
            }
        }

        private void SumByType(IReadOnlyList<Resource> cost)
        {
            Array.Clear(_scratch, 0, _scratch.Length);
            for (var i = 0; i < cost.Count; i++)
                _scratch[(int)cost[i].Type] += cost[i].Amount;
        }

        private void RaiseChanged(ResourceType type)
        {
            var handler = Changed;
            if (handler != null)
                handler(type, _balances[(int)type]);
        }
    }
}

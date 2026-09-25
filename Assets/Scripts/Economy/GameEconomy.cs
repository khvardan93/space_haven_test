using System;
using System.Collections.Generic;
using Configs;

namespace Economy
{
    
    public sealed class GameEconomy
    {
        
        private const double Epsilon = 1e-9;

        private readonly double[] _balances = new double[ResourceTypes.Count];
        private readonly double[] _scratch = new double[ResourceTypes.Count];

        public event Action<ResourceTypeEnum, double> Changed;

        public double Get(ResourceTypeEnum typeEnum)
        {
            return _balances[(int)typeEnum];
        }

        public void Add(ResourceTypeEnum typeEnum, double amount)
        {
            if (amount < 0 || double.IsNaN(amount) || double.IsInfinity(amount))
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Use TrySpend to remove resources.");
            if (amount == 0)
                return;

            _balances[(int)typeEnum] += amount;
            RaiseChanged(typeEnum);
        }

        public void Add(IReadOnlyList<ResourceAmount> amounts)
        {
            for (var i = 0; i < amounts.Count; i++)
                Add(amounts[i].Resource.TypeEnum, amounts[i].Amount);
        }
        
        public void Add(IReadOnlyList<Resource> amounts)
        {
            for (var i = 0; i < amounts.Count; i++)
                Add(amounts[i].TypeEnum, amounts[i].Amount);
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

            for (var i = 0; i < _scratch.Length; i++)
            {
                if (_scratch[i] <= 0)
                    continue;

                _balances[i] = Math.Max(0, _balances[i] - _scratch[i]);
                RaiseChanged((ResourceTypeEnum)i);
            }
            return true;
        }

        public double[] Snapshot()
        {
            return (double[])_balances.Clone();
        }

        public void SetAll(IReadOnlyList<double> balances)
        {
            for (var i = 0; i < _balances.Length; i++)
            {
                var value = balances != null && i < balances.Count ? balances[i] : 0;
                _balances[i] = double.IsNaN(value) || value < 0 ? 0 : value;
                RaiseChanged((ResourceTypeEnum)i);
            }
        }

        private void SumByType(IReadOnlyList<Resource> cost)
        {
            Array.Clear(_scratch, 0, _scratch.Length);
            for (var i = 0; i < cost.Count; i++)
                _scratch[(int)cost[i].TypeEnum] += cost[i].Amount;
        }

        private void RaiseChanged(ResourceTypeEnum typeEnum)
        {
            Changed?.Invoke(typeEnum, _balances[(int)typeEnum]);
        }
    }
}

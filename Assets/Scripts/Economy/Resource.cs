using System;
using System.Collections.Generic;

namespace Economy
{
    public readonly struct Resource
    {
        public readonly ResourceType Type;
        public readonly double Amount;

        public Resource(ResourceType type, double amount)
        {
            if (amount < 0 || double.IsNaN(amount) || double.IsInfinity(amount))
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be a finite non-negative number.");

            Type = type;
            Amount = amount;
        }

        public Resource Scaled(double multiplier)
        {
            return new Resource(Type, Amount * multiplier);
        }

        public override string ToString()
        {
            return Amount + " " + Type;
        }

        public static Resource[] Scale(IReadOnlyList<Resource> source, double multiplier)
        {
            var result = new Resource[source.Count];
            for (int i = 0; i < source.Count; i++)
                result[i] = source[i].Scaled(multiplier);
            return result;
        }
    }
}

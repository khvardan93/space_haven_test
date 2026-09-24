using System;
using System.Collections.Generic;

namespace Economy
{
    public readonly struct Resource
    {
        public readonly ResourceTypeEnum TypeEnum;
        public readonly double Amount;

        public Resource(ResourceTypeEnum typeEnum, double amount)
        {
            if (amount < 0 || double.IsNaN(amount) || double.IsInfinity(amount))
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be a finite non-negative number.");

            TypeEnum = typeEnum;
            Amount = amount;
        }

        public Resource Scaled(double multiplier)
        {
            return new Resource(TypeEnum, Amount * multiplier);
        }

        public override string ToString()
        {
            return Amount + " " + TypeEnum;
        }

        public static Resource[] Scale(IReadOnlyList<Resource> source, double multiplier)
        {
            var result = new Resource[source.Count];
            for (var i = 0; i < source.Count; i++)
                result[i] = source[i].Scaled(multiplier);
            return result;
        }
    }
}

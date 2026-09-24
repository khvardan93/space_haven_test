using System;
using System.Globalization;

namespace View
{
    /// <summary>Idle-game number formatting: 950, 1.2K, 34.5M, 1.20B.</summary>
    public static class NumberFormat
    {
        private static readonly string[] Suffixes = { string.Empty, "K", "M", "B", "T", "Qa", "Qi" };
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        public static string Short(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return "0";

            var negative = value < 0;
            value = Math.Abs(value);

            var tier = 0;
            while (value >= 1000 && tier < Suffixes.Length - 1)
            {
                value /= 1000;
                tier++;
            }

            string number;
            if (tier == 0)
                number = value < 10 && value % 1 > 0.05 ? value.ToString("0.#", Culture) : Math.Floor(value).ToString("0", Culture);
            else if (value < 10)
                number = value.ToString("0.##", Culture);
            else if (value < 100)
                number = value.ToString("0.#", Culture);
            else
                number = value.ToString("0", Culture);

            return (negative ? "-" : "") + number + Suffixes[tier];
        }

        /// <summary>"+2.5/s", "-0.4/s", or "0/s".</summary>
        public static string Rate(double perSecond)
        {
            if (Math.Abs(perSecond) < 0.005)
                return "0/s";

            var sign = perSecond > 0 ? "+" : "-";
            var abs = Math.Abs(perSecond);
            var number = abs < 10 ? abs.ToString("0.0#", Culture)
                          : abs < 1000 ? abs.ToString("0.#", Culture)
                          : Short(abs);
            return $"{sign + number}/s";
        }

        /// <summary>"45s", "3m 05s", "1h 20m".</summary>
        public static string Duration(double seconds)
        {
            if (seconds < 0) seconds = 0;
            var span = TimeSpan.FromSeconds(Math.Ceiling(seconds));

            if (span.TotalHours >= 1)
                return string.Format(Culture, "{0}h {1:00}m", (int)span.TotalHours, span.Minutes);
            if (span.TotalMinutes >= 1)
                return string.Format(Culture, "{0}m {1:00}s", span.Minutes, span.Seconds);
            return string.Format(Culture, "{0}s", span.Seconds);
        }
    }
}

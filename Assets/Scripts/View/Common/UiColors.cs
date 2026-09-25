using UnityEngine;

namespace View
{
    public static class UiColors
    {
        public static readonly Color Positive = new Color(0.42f, 1f, 0.60f, 1f);   // #6BFF9A
        public static readonly Color Negative = new Color(1f, 0.35f, 0.35f, 1f);   // #FF5A5A
        public static readonly Color Neutral = new Color(0.60f, 0.64f, 0.72f, 1f); // #9AA3B8
        public static readonly Color Laser = new Color(1f, 0.76f, 0.24f, 1f);      // #FFC23D, for text
        public static readonly Color LaserFill = new Color(0.88f, 0.54f, 0f, 1f);  // #E08A00, darker so white text stays readable on it

        public const string MissingHex = "#FF5A5A";
        public const string LaserHex = "#FFC23D";

        public static Color ForRate(double perSecond)
        {
            if (perSecond > 0.005) return Positive;
            if (perSecond < -0.005) return Negative;
            return Neutral;
        }
    }
}

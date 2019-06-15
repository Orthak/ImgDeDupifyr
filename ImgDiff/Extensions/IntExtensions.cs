namespace ImgDiff.Extensions
{
    public static class IntExtensions
    {
        public static bool IsBetween(this int source, int lower, int upper, bool inclusive = true)
        {
            return inclusive
                ? lower <= source && source <= upper
                : lower < source && source < upper;
        }

        public static bool InRange(this int source, int target, int rangeFactor, bool inclusive = true)
        {
            return inclusive
                ? (target - rangeFactor) <= source && source <= (target + rangeFactor)
                : (target - rangeFactor) < source && source < (target + rangeFactor);
        }
    }
}

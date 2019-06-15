namespace ImgDiff.Extensions
{
    public static class ByteExtensions
    {
        public static bool IsBetween(this byte source, byte lower, byte upper, bool inclusive = true)
        {
            return inclusive
                ? lower <= source && source <= upper
                : lower < source && source < upper;
        }

        public static bool InRange(this byte source, byte target, int rangeFactor, bool inclusive = true)
        {
            var lower = target - rangeFactor;
            var upper = target + rangeFactor;
            
            if (lower < byte.MinValue)
                lower = byte.MinValue;

            if (upper > byte.MaxValue)
                upper = byte.MaxValue;
            
            return inclusive
                ? lower <= source && source <= upper
                : lower < source && source < upper;
        }
    }
}
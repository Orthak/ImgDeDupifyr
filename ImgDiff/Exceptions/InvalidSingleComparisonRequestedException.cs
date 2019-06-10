using System;

namespace ImgDiff.Exceptions
{
    public class InvalidSingleComparisonRequestedException : Exception
    {
        public InvalidSingleComparisonRequestedException() { }
        
        public InvalidSingleComparisonRequestedException(string message)
            : base(message) { }

        public InvalidSingleComparisonRequestedException(string message, Exception inner)
            : base(message, inner) { }
    }
}

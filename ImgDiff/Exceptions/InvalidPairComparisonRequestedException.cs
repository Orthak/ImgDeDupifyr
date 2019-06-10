using System;

namespace ImgDiff.Exceptions
{
    public class InvalidPairComparisonRequestedException : Exception
    {
        public InvalidPairComparisonRequestedException() { }
        
        public InvalidPairComparisonRequestedException(string message)
            : base(message) { }

        public InvalidPairComparisonRequestedException(string message, Exception inner)
            : base(message, inner) { }
    }
}

using System;

namespace ImgDiff.Exceptions
{
    public class InvalidDirectoryComparisonRequestedException : Exception
    {
        public InvalidDirectoryComparisonRequestedException() { }
        
        public InvalidDirectoryComparisonRequestedException(string message)
            : base(message) { }

        public InvalidDirectoryComparisonRequestedException(string message, Exception inner)
            : base(message, inner) { }
    }
}

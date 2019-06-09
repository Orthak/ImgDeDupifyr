using System;

namespace ImgDiff.Exceptions
{
    public class IncompleteComparisonOptionsException : Exception
    {
        public IncompleteComparisonOptionsException() { }
        
        public IncompleteComparisonOptionsException(string message)
            : base(message) { }

        public IncompleteComparisonOptionsException(string message, Exception inner)
            : base(message, inner) { }
    }
}
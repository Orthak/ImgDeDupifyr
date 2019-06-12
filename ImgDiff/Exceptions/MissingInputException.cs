using System;

namespace ImgDiff.Exceptions
{
    public class MissingInputException : Exception
    {
        public MissingInputException() { }
        
        public MissingInputException(string message)
            : base(message) { }

        public MissingInputException(string message, Exception inner)
            : base(message, inner) { }
    }
}
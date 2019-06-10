using System;

namespace ImgDiff.Exceptions
{
    public class BiasOutOfBoundsException : Exception
    {
        public BiasOutOfBoundsException() { }
        
        public BiasOutOfBoundsException(string message)
            : base(message) { }

        public BiasOutOfBoundsException(string message, Exception inner)
            : base(message, inner) { }
    }
}
using System;

namespace ImgDiff.Exceptions
{
    public class UnsupportedRequestException : Exception
    {        
        public UnsupportedRequestException() { }
        
        public UnsupportedRequestException(string message)
            : base(message) { }

        public UnsupportedRequestException(string message, Exception inner)
            : base(message, inner) { }   
    }
}

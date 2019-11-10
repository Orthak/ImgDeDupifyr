using System;

namespace ImgDiff.Extensions
{
    public static class ExceptionExtensions
    {
        public static void WriteToConsole(this Exception source)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(source.Message);

            if (source.InnerException != null)
            {
                Console.WriteLine("--->");
                WriteToConsole(source.InnerException);
            }
            
            Console.ForegroundColor = ConsoleColor.White;    
        }
    }
}
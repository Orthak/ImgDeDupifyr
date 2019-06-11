using System;
using System.Linq;
using System.Threading.Tasks;
using ImgDiff.Builders;
using ImgDiff.Constants;
using ImgDiff.Interfaces;
using ImgDiff.Models;
using ImgDiff.Monads;
using ImgDiff.Utilities;

namespace ImgDiff
{
    class Program
    {
        // For now, this value won't change during runtime. There may be a case
        // later on that we'll need a different flag parser to handle. 
        static readonly IParseFlags flagsParser = new CommandFlagsParser();

        // Store the main console loop, that we'll be using for the program.
        static readonly MainConsoleLoop mainLoop = new MainConsoleLoop();
        
        static async Task Main(string[] args)
        {
            AddValidFlags();
            var flags = await flagsParser.Parse(args);
            var comparerOptions = new ComparisonOptionsBuilder().FromCommandFlags(flags, new None<ComparisonOptions>());

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("********************");
            Console.WriteLine("*     DEDUPIFYR    *");
            Console.WriteLine("********************");
            Console.WriteLine("Type 'help' for more details.");
            Console.WriteLine("Type 'q' or 'exit' to quit.");

            await mainLoop.Execute(comparerOptions);

            Console.WriteLine("Exiting...");
        }

        /// <summary>
        /// Tell the parser what to look for. Specifying the names, short
        /// and long command strings that should be added to the parser's result.
        /// </summary>
        static void AddValidFlags()
        {
            flagsParser.AddFlag(
                CommandFlagProperties.SearchOptionFlag.Name, 
                CommandFlagProperties.SearchOptionFlag.ShortString,
                CommandFlagProperties.SearchOptionFlag.LongString);
            flagsParser.AddFlag(
                CommandFlagProperties.BiasFactorFlag.Name,
                CommandFlagProperties.BiasFactorFlag.ShortString,
                CommandFlagProperties.BiasFactorFlag.LongString);
        }
    }
}

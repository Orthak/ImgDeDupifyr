using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImgDiff.Builders;
using ImgDiff.Constants;
using ImgDiff.Exceptions;
using ImgDiff.Extensions;
using ImgDiff.Factories;
using ImgDiff.Interfaces;
using ImgDiff.Models;
using ImgDiff.Monads;
using ImgDiff.Utilities;

namespace ImgDiff
{
    public static class Program
    {
        public const string NAME = "DeDupifyr";
        
        // For now, this value won't change during runtime. There may be a case
        // later on that we'll need a different flag parser to handle. 
        static readonly IParseFlags flagsParser = new CommandFlagsParser();

        // Store the main console loop, that we'll be using for the program.
        static MainConsoleLoop mainLoop;
        
        static async Task Main(string[] args)
        {
            AddValidFlags();
            var flags = await flagsParser.Parse(args);
            var comparerOptions = new ComparisonOptionsBuilder().BuildFromFlags(flags, new None<ComparisonOptions>());

            var requestFactory    = new ComparisonRequestFactory();
            var comparisonFactory = new ImageComparisonFactory();
            var statusFactory     = new ExecutionStatusFactory();
            mainLoop = new MainConsoleLoop(
                requestFactory,
                comparisonFactory, 
                statusFactory);

            var helpCommands      = ProgramCommands.ForHelp.ToAggregatedString();
            var terminateCommands = ProgramCommands.ForTermination.ToAggregatedString();
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("********************");
            Console.WriteLine($"*     {NAME.ToUpper()}    *");
            Console.WriteLine("********************");
            Console.WriteLine($"Type {helpCommands} for more details.");
            Console.WriteLine($"Type {terminateCommands} to quit.");

            do
            {
                var executionStatus = statusFactory.ConstructNoOp();
                try
                {
                    executionStatus = await mainLoop.Execute(comparerOptions);
                }
                catch (Exception exception)
                {
                    exception.WriteToConsole();
                }

                if (executionStatus.IsTerminated)
                    break;

                if (executionStatus.IsFaulted)
                {
                    if (executionStatus.FaultedReason.IsSome)
                        executionStatus.FaultedReason.Value.WriteToConsole();
                    else 
                        Console.WriteLine("Unknown exception encountered.");
                    
                    continue;
                }

                if (!executionStatus.IsFaulted)
                    executionStatus.PrintingInstructions();

                // Force the garbage collector to run, after each search
                // session. The idea being that collector will clean up any
                // outstanding resources from the run.
                GC.Collect();
            } while (true);

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
        
        static void HandleNoDuplicates()
        {
            Console.WriteLine($"No duplicate images were found.");
        }
        
        static void HandleHasDuplicates(List<DeDupifyrResult> duplicateResults)
        {
            Console.WriteLine($"The following {duplicateResults.Count} duplicates were found in the request.");
            for (var resultIndex = 0; resultIndex < duplicateResults.Count(); resultIndex++)
            {
                if (!duplicateResults[resultIndex].Duplicates.Any())
                    continue;
                    
                Console.WriteLine($"Result #{resultIndex + 1}: {duplicateResults[resultIndex].SourceImage.Name}");
                for (var dupIndex = 0; dupIndex < duplicateResults[resultIndex].Duplicates.Count(); dupIndex++)
                    Console.WriteLine($"\tDupe #{dupIndex + 1}: {duplicateResults[resultIndex].Duplicates[dupIndex].Image.Name} - {duplicateResults[resultIndex].Duplicates[dupIndex].DuplicationPercent:P}");
            }
        }
    }
}

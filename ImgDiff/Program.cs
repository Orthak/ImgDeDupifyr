using System;
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
        static async Task Main(string[] args)
        {
            // Setup everything we need to run the application.
            var comparerOptions = await BuildComparisonOptions(args);
            var statusFactory   = new ExecutionStatusFactory();
            var mainLoop        = BuildMainLoop(statusFactory);

            // Print the intro splash.
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(ConsoleInterface.GetOpeningSplash());

            // Run the main application loop.
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

                // Force the garbage collector to run, after each search session.
                // The idea being that collector will clean up any outstanding
                // resources from the run.
                GC.Collect();
            } while (true);

            Console.WriteLine("Exiting...");
        }

        static MainConsoleLoop BuildMainLoop(ExecutionStatusFactory statusFactory)
        {
            var requestFactory = new ComparisonRequestFactory();
            var comparisonFactory = new ImageComparisonFactory();
            var mainLoop = new MainConsoleLoop(
                requestFactory,
                comparisonFactory,
                statusFactory);
            
            return mainLoop;
        }

        /// <summary>
        /// Tell the parser what to look for. Specifying the names, short
        /// and long command strings that should be added to the parser's result.
        /// </summary>
        static async Task<ComparisonOptions> BuildComparisonOptions(string[] args)
        {
            var flagsParser = new CommandFlagsParser()
                .AddFlag(CommandFlagProperties.SearchOptionFlag)
                .AddFlag(CommandFlagProperties.StrictnessFlag)
                .AddFlag(CommandFlagProperties.BiasFactorFlag);
            
            var flags           = await flagsParser.Parse(args);
            var comparerOptions = new ComparisonOptionsBuilder().BuildFromFlags(flags, new None<ComparisonOptions>());

            return comparerOptions;
        }
    }
}

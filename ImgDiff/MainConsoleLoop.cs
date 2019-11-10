using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ImgDiff.Builders;
using ImgDiff.Constants;
using ImgDiff.Exceptions;
using ImgDiff.Extensions;
using ImgDiff.Factories;
using ImgDiff.Models;
using ImgDiff.Monads;

namespace ImgDiff
{
    public class MainConsoleLoop
    {
        readonly ComparisonRequestFactory requestFactory;
        readonly ImageComparisonFactory comparisonFactory;
        readonly ExecutionStatusFactory statusFactory;

        public MainConsoleLoop(
            ComparisonRequestFactory injectedRequestFactory,
            ImageComparisonFactory injectedComparisonFactory,
            ExecutionStatusFactory injectedStatusFactory)
        {
            requestFactory    = injectedRequestFactory;
            comparisonFactory = injectedComparisonFactory;
            statusFactory     = injectedStatusFactory;
        }
        
        public async Task<ExecutionStatus> Execute(ComparisonOptions initialOptions)
        {
            Console.Write($"{AppProperties.Name}> ");
            
            var inputString = Console.ReadLine();
            if (string.IsNullOrEmpty(inputString))
            {
                return statusFactory.ConstructFaulted(
                    new MissingInputException(
                        "You must enter either a directory ('C:\\to\\some\\directory'), or a pair of files separated by a coma ('C:\\path\\to\\first.png,C:\\path\\to\\second.jpg')."));
            }
            
            if (CommandIsGiven(inputString, ProgramCommands.ForTermination))
                return statusFactory.ConstructTerminated();

            if (CommandIsGiven(inputString, ProgramCommands.ForHelp))
            {
                Console.WriteLine(ConsoleInterface.GetHelpText());

                return statusFactory.ConstructNoOp();
            }
            
            var comparisonRequest = requestFactory.Construct(inputString);
            if (CommandIsGiven(inputString, ProgramCommands.ToChangeOptions))
            {
                initialOptions = OverwriteComparisonOptions(initialOptions);
                
                // We need to return here, so that we can reset the console 
                // input. Otherwise we'll accidentally attempt to process "options"
                // as a directory.
                return statusFactory.ConstructUpdated(comparisonRequest, initialOptions);
            }

            var imageComparer = comparisonFactory.Construct(comparisonRequest, initialOptions);

            Option<List<DeDupifyrResult>> duplicateResults = new None<List<DeDupifyrResult>>();

            Action printInstructions;
            
            // For now, just doing a try/catch at the highest level. I plan
            // to have much better handling. I'll implement the `Either` monad
            // and have the `Run` method return an instance of that. From there,
            // I can determine whether to write an error out, or to write the results.
            try
            {
                var sw = Stopwatch.StartNew();
                var results = await imageComparer.Run(comparisonRequest);
                sw.Stop();
                
                Console.WriteLine($"Done in {sw.ElapsedMilliseconds} ms.");
                
                duplicateResults = new Some<List<DeDupifyrResult>>(results);
                printInstructions = imageComparer.PrintInstructions();
            }
            catch (Exception exception)
            {
                return statusFactory.ConstructFaulted(exception);
            }

            return statusFactory.ConstructSuccess(
                comparisonRequest,
                duplicateResults,
                printInstructions);
        }
        
        static bool CommandIsGiven(string input, IEnumerable<string> toCheck) =>
            toCheck.Any(command => 
                input.Trim()
                    .ToLowerInvariant()
                    .Equals(command));
        
        /// <summary>
        /// Change the options that the programs performs comparisons with. The
        /// user will be prompted for each option to overwrite, one at a time.
        /// </summary>
        /// <param name="currentOptions">The options comparisons are currently using.</param>
        /// <returns>The new options future comparisons should run with.</returns>
        static ComparisonOptions OverwriteComparisonOptions(ComparisonOptions currentOptions)
        {
            Console.WriteLine("Enter New Option Values");
            Console.WriteLine("Leave an option blank to keep its current value.");
            var flagsToChange = new Dictionary<string, string>();
            
            // Ask for how deep to look in the directory. If the user does not input a value,
            // we keep the current setting.
            Console.WriteLine("Directory Level: ");
            var newDirectoryLevel = Console.ReadLine();
            if (!string.IsNullOrEmpty(newDirectoryLevel))
                flagsToChange[CommandFlagProperties.SearchOptionFlag.Name] = newDirectoryLevel;
            
            // Ask for the new strictness level to use. No value means we keep the current
            // value we have.
            Console.WriteLine("Strictness Level: ");
            var newStrictness = Console.ReadLine();
            if (!string.IsNullOrEmpty(newStrictness))
                flagsToChange[CommandFlagProperties.StrictnessFlag.Name] = newStrictness;
            
            // Ask for the new bias factor. The current setting is kept if the user gives
            // no new value.
            Console.WriteLine("Bias Factor: ");
            var newBiasFactor = Console.ReadLine();
            if (!string.IsNullOrEmpty(newBiasFactor))
                flagsToChange[CommandFlagProperties.BiasFactorFlag.Name] = newBiasFactor;

            // Build a new `ComparisonOptions` object, using the newly created dictionary
            // from the user's input.
            var updatedOptions = new ComparisonOptionsBuilder()
                .BuildFromFlags(flagsToChange, new Some<ComparisonOptions>(currentOptions));

            return updatedOptions;
        }
    }
}

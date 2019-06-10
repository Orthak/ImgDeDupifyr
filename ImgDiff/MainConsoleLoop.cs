using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ImgDiff.Builders;
using ImgDiff.Constants;
using ImgDiff.Factories;
using ImgDiff.Models;
using ImgDiff.Monads;

namespace ImgDiff
{
    public class MainConsoleLoop
    {
        ComparisonRequestFactory requestFactory  = new ComparisonRequestFactory();
        ImageComparisonFactory comparisonFactory = new ImageComparisonFactory();
        
        public async Task Execute(ComparisonOptions initialOptions)
        {
            do
            {
                Console.Write("DeDupifyr:> ");
                var inputString = Console.ReadLine();
                if (string.IsNullOrEmpty(inputString))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(
                        "You must enter either a directory ('C:\\to\\some\\directory'), or a pair of files separated by a coma ('C:\\path\\to\\first.png,C:\\path\\to\\second.jpg').");
                    Console.ForegroundColor = ConsoleColor.White;

                    continue;
                }

                if (ShouldTerminate(inputString))
                    break;

                if (inputString.ToLowerInvariant().Equals("options"))
                {
                    initialOptions = OverwriteComparisonOptions(initialOptions);
                    
                    // We need to continue here, so that we can reset the console 
                    // input. Otherwise we'll accidentally attempt to process "options"
                    // as a directory.
                    continue;
                }

                var comparisonRequest = requestFactory.ConstructNew(inputString);
                var imageComparer     = comparisonFactory.ConstructNew(comparisonRequest, initialOptions);

                List<DuplicateResult> duplicateResults;
                
                // For now, just doing a try/catch at the highest level. I plan
                // to have much better handling. I'll implement the `Either` monad
                // and have the `Run` method return an instance of that. From there,
                // I can determine whether to write an error out, or to write the results.
                try
                {
                    var sw = Stopwatch.StartNew();
                    duplicateResults = await imageComparer.Run(comparisonRequest);
                    sw.Stop();

                    Console.WriteLine($"Done in {sw.ElapsedMilliseconds} ms.");
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    
                    continue;
                }
                
                if (duplicateResults.Count <= 0)
                    HandleNoDuplicates(inputString);
                else
                    HandleHasDuplicates(duplicateResults, inputString);
                
                // Force the garbage collector to run, after each search
                // session. The idea being that collector will clean up any
                // outstanding resources from the run.
                GC.Collect();
            } while (true);
        }

        static bool ShouldTerminate(string request)
        {
            return ProgramCommands.ForTermination
                .Any(command => 
                    request.Trim()
                           .ToLowerInvariant()
                           .Equals(command));
        }

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
            
            // Ask for the new bias factor. The current setting is kept if the user gives
            // no new value.
            Console.WriteLine("Bias Factor: ");
            var newBiasFactor = Console.ReadLine();
            if (!string.IsNullOrEmpty(newBiasFactor))
                flagsToChange[CommandFlagProperties.BiasFactorFlag.Name] = newBiasFactor;

            // Build a new `ComparisonOptions` object, using the newly created dictionary
            // from the user's input.
            var updatedOptions = new ComparisonOptionsBuilder()
                .FromCommandFlags(flagsToChange, new Some<ComparisonOptions>(currentOptions));

            return updatedOptions;
        }
        
        static void HandleNoDuplicates(string requestedDirectory)
        {
            Console.WriteLine($"No duplicate images were found in '{requestedDirectory}'.");
        }
        
        static void HandleHasDuplicates(List<DuplicateResult> duplicateResults, string requestedDirectory)
        {
            Console.WriteLine($"The following {duplicateResults.Count} duplicates were found in '{requestedDirectory}':");
            for (var resultIndex = 0; resultIndex < duplicateResults.Count(); resultIndex++)
            {
                if (!duplicateResults[resultIndex].Duplicates.Any())
                    continue;
                    
                Console.WriteLine($"Result #{resultIndex}: {duplicateResults[resultIndex].BaseImage.Name}");
                for (var dupIndex = 0; dupIndex < duplicateResults[resultIndex].Duplicates.Count(); dupIndex++)
                    Console.WriteLine($"\tDupe #{dupIndex}: {duplicateResults[resultIndex].Duplicates[dupIndex].Name}");
            }
        }
    }
}

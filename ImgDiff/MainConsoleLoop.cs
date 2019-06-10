using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImgDiff.Builders;
using ImgDiff.Comparers;
using ImgDiff.Constants;
using ImgDiff.Hashing;
using ImgDiff.Interfaces;
using ImgDiff.Models;
using ImgDiff.Monads;

namespace ImgDiff
{
    public class MainConsoleLoop
    {
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

                var comparisonRequest = BuildComparisonRequest(inputString);
                var imageComparer     = BuildImageComparer(comparisonRequest, initialOptions);

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
                    request.Trim().ToLowerInvariant().Equals(command));
        }
        
        /// <summary>
        /// To help better represent what the user requested, and perform better checks
        /// against the request, we build a new object to hold this data.
        /// </summary>
        /// <param name="request">The raw string request that was given by the user.</param>
        /// <returns>The object representation of the user's request.</returns>
        static ComparisonRequest BuildComparisonRequest(string request)
        {
            var requestBuilder = new ComparisonRequestBuilder();
            if (request.Contains(','))
            {
                var requestArgs = request.Split(',');
                if (Directory.Exists(requestArgs[1]))
                {
                    requestBuilder.UsingComparisonAs(ComparisonWith.Single)
                        .InDirectory(requestArgs[1])
                        .WithFirstImage(requestArgs[0]);
                }
                else
                {
                    requestBuilder.UsingComparisonAs(ComparisonWith.Pair)
                        .WithFirstImage(requestArgs[0])
                        .WithSecondImage(requestArgs[1]);
                }
            }
            else
            {
                requestBuilder.UsingComparisonAs(ComparisonWith.All)
                    .InDirectory(request);
            }

            var comparisonRequest = requestBuilder.Build().Validate();
            return comparisonRequest;
        }

        /// <summary>
        /// Construct the comparison object that we'll be using for this current run. Once
        /// we have it, we need only call the `Run` method on the result.
        /// </summary>
        /// <param name="request"><see cref="ComparisonRequest"/>: The representation of the current request.</param>
        /// <param name="options"><see cref="ComparisonOptions"/>: The options that will be used for the request.</param>
        /// <returns>The comparison object, that will handle the current request.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if we get a <see cref="ComparisonWith"/>
        /// value that isn't recognized.</exception>
        static ICompareImages BuildImageComparer(ComparisonRequest request, ComparisonOptions options)
        {
            ICompareImages imageComparer;
            IHashProvider hashProvider = new BasicHashProvider();

            switch (request.With)
            {
                case ComparisonWith.All:
                    imageComparer = new DirectoryComparison(
                        hashProvider,
                        new HashComparison(options.BiasPercent),
                        options);
                    break;
                case ComparisonWith.Pair:
                    imageComparer = new PairComparison(
                        hashProvider,
                        new LevenshteinComparison(),
                        options);
                    break;
                case ComparisonWith.Single:
                    imageComparer = new SingleComparison(
                        hashProvider,
                        new HashComparison(options.BiasPercent),
                        options);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return imageComparer;
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

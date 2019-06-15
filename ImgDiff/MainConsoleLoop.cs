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
            Console.Write($"{Program.NAME}> ");
            
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
                OutputHelpText();

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

        
        static bool CommandIsGiven(string input, IEnumerable<string> toCheck)
        {
            return toCheck.Any(command => 
                input.Trim()
                    .ToLowerInvariant()
                    .Equals(command));
        }
        
        static void OutputHelpText()
        {
            var validExtensionsCombined = ValidExtensions.ForImage.ToAggregatedString();
            var changeOptionCommands    = ProgramCommands.ToChangeOptions.ToAggregatedString();

            var helpText = 
$@" *** {Program.NAME} ***
_____ABOUT
This application is used to discover and determine duplicate images.
This is done in 1 of 3 'request' types. Only 1 request can be active at a given time.
The 3 request types are 'Directory', 'Single', and 'Pair'.
Directory - Compares all images in a directory with every other.
Single    - Compares 1 image with all other images in a given directory.
Pair      - Compares 2 different images with each other.

_____FORMATS
Requests can be made using the following formats
    (Directory)              '/path/to/some/directory'
    (Image with Directory)   '/path/to/image.[extension] , /directory/to/compare/against'
    (Image with Other Image) '/path/to/image1.[extension] , /path/to/image2.[extension]'
Valid extensions are {validExtensionsCombined}.
Other extension types will simply be ignore by the application.

_____OPTIONS
There are currently 3 options that can be set.
Directory Level: Tells the program how deep in the directory to search. Does not apply to the Singe request type.
    Values: all, [top]
Strictness: How equal the pixel colors must be, to consider them to be equal.
    Values: Equal, [Fuzzy], Loose
Bias Factor: The percentage that a comparison must equal, or exceed, for an image to be considered a duplicate.
    Values: 0 to 100, [80]
Type {changeOptionCommands} to overwrite the current option settings.
";
            
            Console.WriteLine(helpText);
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

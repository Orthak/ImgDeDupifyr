using ImgDiff.Extensions;

namespace ImgDiff.Constants
{
    public static class ConsoleInterface
    {
        public static string GetOpeningSplash()
        {
            var helpCommands      = ProgramCommands.ForHelp.ToAggregatedString();
            var terminateCommands = ProgramCommands.ForTermination.ToAggregatedString();

            var nameSplash =
$@"********************
*     {AppProperties.Name}     *
********************";
            var helpSplash = $"Type {helpCommands} for more details.";
            var exitSplash = $"Type {terminateCommands} to quit.";

            return $"{nameSplash}\n{helpSplash}\n{exitSplash}";
        }

        public static string GetHelpText()
        {
            var validExtensionsCombined = ValidExtensions.ForImage.ToAggregatedString();
            var changeOptionCommands    = ProgramCommands.ToChangeOptions.ToAggregatedString();
            
            var helpText =
$@" *** {AppProperties.Name} ***
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
(The `[` and `]` around a parameter value denotes it as the default value, if none is supplied explicitly.)
";
            return helpText;
        }
    }
}

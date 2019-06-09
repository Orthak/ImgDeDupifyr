using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImgDiff.Builders;
using ImgDiff.Comparers;
using ImgDiff.Hashing;
using ImgDiff.Interfaces;
using ImgDiff.Models;

namespace ImgDiff
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(
                "Enter the directory, to check for duplicate images. Or enter 2 files to compare, separated by a comma.");

            var request = Console.ReadLine();
            if (string.IsNullOrEmpty(request))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("You must enter either a directory, or a pair of files separated by a coma.");
                Console.ForegroundColor = ConsoleColor.White;

                return;
            }

            var imageComparer = BuildImageComparer(request);
            var duplicateResults = await imageComparer.Run(request);
            if (duplicateResults.Count <= 0)
                HandleNoDuplicates(request);
            else
                HandleHasDuplicates(duplicateResults, request);
        }

        static ICompareImages BuildImageComparer(string request)
        {
            ICompareImages imageComparer;
            IHashProvider hashProvider = new BasicHashProvider();
            ComparisonOptions comparerOptions = new ComparisonOptionsBuilder()
                .SearchOnlyTopDirectory(true)
                .ShouldSucceedWithPercentage(1/(double)9)
                .Build();
            ICompareStrings stringComparer = new HashComparison(comparerOptions.BiasPercent);
            
            if (request.Contains(","))
                imageComparer = new SingleComparison(
                    hashProvider,
                    stringComparer,
                    comparerOptions);
            else
                imageComparer = new DirectoryComparison(
                    hashProvider,
                    stringComparer,
                    comparerOptions);
            
            return imageComparer;
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

using System;
using System.Linq;
using System.Threading.Tasks;

namespace ImgDiff
{
    class Program
    {
        static ICompareImages comparer;
        
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

            if (request.Contains(","))
            {
                comparer = new SingleComparison();
            }
            else
            {
                comparer = new DirectoryComparison();
            }
            
            var duplicateResults = await comparer.Run(request);

            Console.WriteLine($"The following duplicates were found:");
            for (var resIndex = 0; resIndex < duplicateResults.Count(); resIndex++)
            {
                if (!duplicateResults[resIndex].Duplicates.Any())
                    continue;
                
                Console.WriteLine($"Result #{resIndex}: {duplicateResults[resIndex].BaseImage.Name}");
                for (var dupIndex = 0; dupIndex < duplicateResults[resIndex].Duplicates.Count(); dupIndex++)
                {
                    Console.WriteLine($"\tDupe #{dupIndex}: {duplicateResults[resIndex].Duplicates[dupIndex].Name}");
                }
            }
        }
    }
}

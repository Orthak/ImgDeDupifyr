using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImgDiff.Interfaces;
using ImgDiff.Models;
using ImgDiff.Monads;

namespace ImgDiff.Comparers.ForImages
{
    /// <summary>
    /// Compare 2 different images, specifically. 
    /// </summary>
    public class PairComparison : ImageComparer, ICompareImages
    {
        public PairComparison(
            IHashProvider injectedHashProvider,
            ICompareStrings stringComparer,
            ComparisonOptions options)
        : base(injectedHashProvider, stringComparer, options)
        { }
        
        public async Task<List<DeDupifyrResult>> Run(ComparisonRequest request)
        {
            // If the paths are equal, print that we were given the same image path.
            if (request.FirstImagePath.Value == request.SecondImagePath.Value)
                Console.WriteLine("The two paths given point to the same image.");

            Console.WriteLine($"Comparing images at {request.FirstImagePath.Value} and {request.SecondImagePath.Value}...");
            
            var sourceImage = await BuildLocalImage(request.FirstImagePath.Value);
            var targetImage = await BuildLocalImage(request.SecondImagePath.Value);

            var duplicateResult = new DeDupifyrResult(sourceImage);
            await UpdateDuplicationCollection(
                sourceImage,
                targetImage,
                duplicateResult.Duplicates);
            
            duplicateResults.Add(duplicateResult);
            
            return duplicateResults;
        }

        protected override async Task<Option<DuplicateImage>> DirectComparison(LocalImage source, LocalImage target)
        {
            var percentageEquivalent = await hashComparer.CalculatePercentage(
                source.Hash, 
                target.Hash);
            
            return new Some<DuplicateImage>(new DuplicateImage(target, percentageEquivalent));
        }

        protected override Task<string> GetFileHash(string filePath)
        {
            // Because our sample size is so small (only 2 images at a time), we can
            // afford to read all the file's bytes.
            var bytes = File.ReadAllBytes(filePath);

            return hashProvider.CreateHash(bytes);
        }

        public override Action PrintInstructions() => () =>
        {
            if (!duplicateResults.Any())
                return;
            
            var result = duplicateResults[0];
            var duplicates = result.Duplicates;
            Console.WriteLine(
                !duplicates.Any()
                ? $"The images are not similar in any way."
                : $"The images are similar by approximately {duplicateResults[0].Duplicates[0].DuplicationPercent:P}");
        };
    }
}

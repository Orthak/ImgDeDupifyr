using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImgDiff.Constants;
using ImgDiff.Interfaces;
using ImgDiff.Models;
using ImgDiff.Monads;

namespace ImgDiff.Comparers.ForImages
{
    /// <summary>
    /// Compare a single image file against all other image files in
    /// the given directory, and optionally in all the sub-directories.  
    /// </summary>
    public class SingleComparison : ImageComparer<string>, ICompareImages
    {
        public SingleComparison(
            IHashProvider provider,
            ICalculateDifference<string> stringComparer,
            ComparisonOptions options) 
        : base(provider, stringComparer, options)
        { }

        public async Task<List<DeDupifyrResult>> Run(ComparisonRequest request)
        {
            Console.WriteLine($"Comparing the image at '{request.FirstImagePath.Value}' with all images in '{request.DirectoryPath.Value}'...");

            var sourceImage = await BuildLocalImage(request.FirstImagePath.Value);
            var otherImageBuilders = ImageBuildersFromDirectory(
                    request.DirectoryPath.Value,
                    file => ValidExtensions.ForImage.Contains(Path.GetExtension(file)) 
                               && file != request.FirstImagePath.Value);

            var otherImages = await Task.WhenAll(otherImageBuilders);

            var duplicateResult = new DeDupifyrResult(sourceImage);
            var duplicateImages = await CompareToOthers(sourceImage, otherImages);
            duplicateResult.Duplicates.AddRange(duplicateImages);
            
            duplicateResults.Add(duplicateResult);

            return duplicateResults;
        }

        protected override async Task<Option<DuplicateImage>> DirectComparison(LocalImage source, LocalImage target)
        {
            var directResult = await base.DirectComparison(source, target);
            if (directResult.IsSome)
                return directResult;

            var percentage = await differenceCalculator.CalculatePercentage(
                source.Hash,
                target.Hash);
            if (percentage >= comparisonOptions.BiasPercent)
                return new Some<DuplicateImage>(new DuplicateImage(target, percentage));
            
            return new None<DuplicateImage>();
        }

        async Task<DuplicateImage[]> CompareToOthers(LocalImage source, LocalImage[] targets)
        {
            var duplicates = new List<DuplicateImage>();
            for (var index = 0; index < targets.Length; index++)
            {
                await UpdateDuplicationCollection(
                    source,
                    targets[index],
                    duplicates);
            }

            return duplicates.ToArray();
        }
        
        protected override Task<string> GetFileHash(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            var lengthReductionModifier = 1 << 3;
            var bytesToHash = bytes.Take(bytes.Length / lengthReductionModifier).ToArray();

            return hashProvider.CreateHash(bytesToHash);
        } 
        
        public override Action PrintInstructions() => () =>
        {
            var givenImage = duplicateResults[0];
            if (givenImage.Duplicates.Count <= 0)
                Console.WriteLine($"No images in the directory are duplicates of the given image.");
            else
            {
                Console.WriteLine($"The following {givenImage.Duplicates.Count} duplicates were found in the directory.");
                for (var dupIndex = 0; dupIndex < givenImage.Duplicates.Count(); dupIndex++)
                    Console.WriteLine(
                        $"\tDupe #{dupIndex + 1}: {givenImage.Duplicates[dupIndex].Image.Name} - {givenImage.Duplicates[dupIndex].DuplicationPercent:P}");
            }
        };
    }
}

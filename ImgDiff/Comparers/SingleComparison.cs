using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImgDiff.Constants;
using ImgDiff.Interfaces;
using ImgDiff.Models;

namespace ImgDiff.Comparers
{
    /// <summary>
    /// Compare a single image file against all other image files in
    /// the given directory, and optionally in all the sub-directories.  
    /// </summary>
    public class SingleComparison : ImageComparer, ICompareImages
    {
        public SingleComparison(
            IHashProvider provider,
            ICompareStrings stringComparer,
            ComparisonOptions options) 
        : base(provider, stringComparer, options)
        { }

        public async Task<List<DuplicateResult>> Run(ComparisonRequest request)
        {
            Console.WriteLine($"Comparing the image at '{request.FirstImagePath.Value}' with all images in '{request.DirectoryPath.Value}'...");

            var sourceImage = await BuildLocalImage(request.FirstImagePath.Value);
            var otherImageBuilders = ImageBuildersFromDirectory(
                    request.DirectoryPath.Value,
                    file => ValidExtensions.ForImage.Contains(Path.GetExtension(file)) 
                               && file != request.FirstImagePath.Value);

            var otherImages = await Task.WhenAll(otherImageBuilders);

            var duplicateResult = new DuplicateResult(sourceImage);
            var duplicateImages = await CompareToOthers(sourceImage, otherImages);
            duplicateResult.Duplicates.AddRange(duplicateImages);
            
            duplicateResults.Add(duplicateResult);

            return duplicateResults;
        }

        async Task<LocalImage[]> CompareToOthers(LocalImage source, LocalImage[] targets)
        {
            var duplicates = new List<LocalImage>();
            for (var index = 0; index < targets.Length; index++)
            {
                var areEqual = await DirectComparison(source, targets[index]);
                if (!areEqual)
                    continue;
                
                duplicates.Add(targets[index]);
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
    }
}

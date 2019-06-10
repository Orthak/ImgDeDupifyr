using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgDiff.Constants;
using ImgDiff.Hashing;
using ImgDiff.Interfaces;
using ImgDiff.Models;

namespace ImgDiff.Comparers
{
    /// <summary>
    /// Compare each image in a given directory will all other images
    /// in that directory. Can also optionally look through all sub-directories.
    /// </summary>
    public class DirectoryComparison : ImageComparer, ICompareImages
    {
        public DirectoryComparison(
            IHashProvider injectedHashProvider,
            ICompareStrings stringComparer,
            ComparisonOptions options)
        : base (injectedHashProvider, stringComparer, options)
        { }
        
        public async Task<List<DuplicateResult>> Run(ComparisonRequest request)
        {
            Console.WriteLine($"Searching {(comparisonOptions.DirectorySearchOption == SearchOption.TopDirectoryOnly ? "only" : "the top of, and all sub directories, ")} in {request.DirectoryPath.Value}...");
            
            var imageFileBuilders = ImageBuildersFromDirectory(request.DirectoryPath.Value,
                    file => ValidExtensions.ForImage.Contains(Path.GetExtension(file)));
            
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            // Fire off all the tasks to build the LocalImages array, and wait for them all to complete.
            // Doing it this way means that we only have to wait as long as the longest running builder,
            // instead of having to wait for each one additively.
            var images = await Task.WhenAll(imageFileBuilders);
#if DEBUG
            sw.Stop();
            Console.WriteLine($"Took {sw.ElapsedMilliseconds} ms to run all image builders.");
#endif
            
            Console.WriteLine($"Checking {images.Count()} files in '{request.DirectoryPath.Value}'...");
#if DEBUG
            sw.Restart();
#endif
            await CheckForDuplicates(images);
#if DEBUG
            sw.Stop();
            Console.WriteLine($"Took {sw.ElapsedMilliseconds} ms to check all duplicates.");
#endif

            return duplicateResults;
        }

        async Task CheckForDuplicates(LocalImage[] inDirectory)
        {
            // Here, we create a list that contains all the names of files
            // that we already processed. This is so we don't re-process the
            // same images, when we pass over them again.
            var visited = new List<string>();
            
            // Using a `for` loop here against the LocalImage array, to focus
            // more on performance.
            for (var index = 0; index < inDirectory.Length; index++)
            {
                if (visited.Contains(inDirectory[index].Name))
                    continue;

                var duplicates = await VisitOthers(inDirectory, inDirectory[index], visited);
                if (duplicates.Any())
                {
                    var duplicateResult = new DuplicateResult(inDirectory[index]);
                    duplicateResult.Duplicates.AddRange(duplicates);
                    duplicateResults.Add(duplicateResult);
                }

                visited.Add(inDirectory[index].Name);
            }
            
            // We no longer need to keep the visited list in scope, 
            // clear it out.
            visited.Clear();
        }

        /// <summary>
        /// For the current image that we're checking, check all other files
        /// in that directory (or also all subdirectories, if requested) for
        /// duplication.
        /// </summary>
        /// <param name="inDirectory"></param>
        /// <param name="localImage"></param>
        /// <param name="visited"></param>
        /// <returns></returns>
        Task<LocalImage[]> VisitOthers(
            LocalImage[] inDirectory,
            LocalImage localImage,
            List<string> visited) =>
            Task.Run(async () =>
            {
                var duplicates = new List<LocalImage>();
                for (var index = 0; index < inDirectory.Length; index++)
                {
                    // If we've already processed the image at some point,
                    // skip it.
                    if (inDirectory[index].Name == localImage.Name)
                        continue;

                    var areEqual = await DirectComparison(localImage, inDirectory[index]);
                    if (!areEqual) 
                        continue;
                    
                    duplicates.Add(inDirectory[index]);
                    visited.Add(inDirectory[index].Name);
                }

                return duplicates.ToArray();
            });

        protected override Task<string> GetFileHash(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            var lengthReductionModifier = 1 << 4;
            var bytesToHash = bytes.Take(bytes.Length / lengthReductionModifier).ToArray();
            
            return hashProvider.CreateHash(bytesToHash);
        }
    }
}

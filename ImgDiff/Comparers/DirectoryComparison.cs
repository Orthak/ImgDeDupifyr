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
    public class DirectoryComparison : ImageComparer, ICompareImages
    {
        readonly List<DuplicateResult> duplicateResults = new List<DuplicateResult>();

        public DirectoryComparison(
            IHashProvider injectedHashProvider,
            ICompareStrings stringComparer, 
            ComparisonOptions options)
        : base (injectedHashProvider, stringComparer, options)
        { }
        
        public async Task<List<DuplicateResult>> Run(string requestedDirectory)
        {
            if (Directory.Exists(requestedDirectory) == false)
                throw new DirectoryNotFoundException(
                    $"Directory '{requestedDirectory}' was not found on the disk.");

            Console.WriteLine($"Searching {(comparisonOptions.TopDirectoryOnly ? "only" : "the top of, and all sub directories, ")} in {requestedDirectory}...");
            
            // Here, we use the `.EnumerateFiles` method, so that we don't have to wait for 
            // every item to be return to actually start processing the collection. This improves
            // performance quite a bit, as we can start building and processing the LocalImage
            // collection as soon as possible.
            // We also return the `Task` of building the local image, so we can process them all 
            // separately, which also improves the speed of the application considerably (~1.9ms for ~2,000 image files).
            var imageFileBuilders = Directory.EnumerateFiles(
                requestedDirectory,
                "*", 
                comparisonOptions.TopDirectoryOnly?
                SearchOption.TopDirectoryOnly 
                : SearchOption.AllDirectories)
                .Where(file => ValidExtensions.ForImage.Contains(Path.GetExtension(file)))
                .Select(file => BuildLocalImage(file, Path.GetFileName(file), Path.GetExtension(file)));
            
            // Fire off all the tasks to build the LocalImages array, and wait for them all to complete.
            // Doing it this way means that we only have to wait as long as the longest running builder,
            // instead of having to wait for each one additively.
            var sw = Stopwatch.StartNew();
            var images = await Task.WhenAll(imageFileBuilders);
            sw.Stop();
            Console.WriteLine($"Took {sw.ElapsedMilliseconds} ms to run all builders.");

            Console.WriteLine($"Checking {images.Count()} files in '{requestedDirectory}'...");
            sw.Restart();
            await CheckForDuplicates(images);
            sw.Stop();
            Console.WriteLine($"Took {sw.ElapsedMilliseconds} ms to check all files.");

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

        Task<LocalImage[]> VisitOthers(
            LocalImage[] inDirectory,
            LocalImage localImage,
            List<string> visited) =>
            Task.Run(() =>
            {
                var duplicates = new List<LocalImage>();
                for (var index = 0; index < inDirectory.Length; index++)
                {
                    // If we've already processed the image at some point,
                    // skip it.
                    if (inDirectory[index].Name == localImage.Name)
                        continue;

                    // TODO:
                    // The hashes are too large to use this method. I'll need
                    // to figure something else out, to get a better performing
                    // way to compare the strings.
                    ////var differencePercent = hashComparer.CalculatePercentage(
                    ////    localImage.Hash,
                    ////    inDirectory[index].Hash);
                    ////
                    ////// If the differencePercent is NOT greater than the bias option,
                    ////// continue to the next check.
                    ////if (differencePercent < comparisonOptions.BiasPercent)
                    ////    continue;
                    
                    // Add the found duplicate to the list of duplicates for the
                    // LocalImage we're comparing against.          
                    var percentage = hashComparer.CalculatePercentage(
                        localImage.Hash,
                        inDirectory[index].Hash);
                    if (localImage.Hash == inDirectory[index].Hash
                        || percentage >= comparisonOptions.BiasPercent)
                    {
                        duplicates.Add(inDirectory[index]);
                        visited.Add(inDirectory[index].Name);
                    }
                }

                return duplicates.ToArray();
            });

        /// <summary>
        /// Instantiate a new custom class, to hold some useful data about
        /// each image that we want to look at. This helps reduce complex
        /// computation later, when we loop over everything.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        async Task<LocalImage> BuildLocalImage(string filePath, string name, string extension)
        {
            var hash = await GetFileHash(filePath);
            
            return new LocalImage(
                name,
                filePath,
                extension,
                hash);
        }
        
        Task<string> GetFileHash(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            var toHash = bytes.Take(bytes.Length / 16).ToArray();
            
            return hashProvider.Generate(toHash);
        }
    }
}
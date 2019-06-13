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
    public abstract class ImageComparer
    {
        protected readonly IHashProvider hashProvider;
        protected readonly ICompareStrings hashComparer;
        protected readonly ComparisonOptions comparisonOptions;

        protected readonly List<DeDupifyrResult> duplicateResults = new List<DeDupifyrResult>();

        protected ImageComparer(
            IHashProvider provider,
            ICompareStrings stringComparer,
            ComparisonOptions options)
        {
            hashProvider = provider;
            hashComparer = stringComparer;
            comparisonOptions = options;
        }
        
        /// <summary>
        /// Return an enumerable of the `BuildLocalImage` task, so that we can build
        /// all the images we need all at once.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="wherePredicate"></param>
        /// <returns>The enumerable of tasks to build the <see cref="LocalImage"/> objects.</returns>
        protected IEnumerable<Task<LocalImage>> ImageBuildersFromDirectory(string directoryPath, Func<string, bool> wherePredicate)
        {
            // Here, we use the `.EnumerateFiles` method, so that we don't have to wait for 
            // every item to be return to actually start processing the collection. This improves
            // performance quite a bit, as we can start building and processing the LocalImage
            // collection as soon as possible.
            return Directory.EnumerateFiles(
                    directoryPath,
                    "*",
                    comparisonOptions.DirectorySearchOption)
                .Where(wherePredicate)
                .Select(BuildLocalImage);
        }
        
        /// <summary>
        /// Instantiate a new custom class, to hold some useful data about
        /// each image that we want to look at. This helps reduce complex
        /// computation later, when we loop over everything.
        /// </summary>
        /// <param name="filePath">The path of the file to build from.</param>
        /// <returns>The <see cref="LocalImage"/> representation of file at the given path.</returns>
        protected async Task<LocalImage> BuildLocalImage(string filePath)
        {
            var name = Path.GetFileName(filePath);
            var extension = Path.GetExtension(filePath);
            var hash = await GetFileHash(filePath);
            
            return new LocalImage(
                name,
                filePath,
                extension,
                hash);
        }

        protected virtual async Task<Option<DuplicateImage>> DirectComparison(LocalImage source, LocalImage target)
        {
            // First, we check for 100% equality. This is much faster when
            // C# gets to do it's own thing. Doing it this way also means
            // we don't have to got into generating the equality percent if
            // we don't need to. If they aren't fully equal, we move on to
            // hash comparison, using the Bias Factor in our options.
            if (source.Hash == target.Hash)
                return new Some<DuplicateImage>(new DuplicateImage(target, 1.0));

            // If they're not 100% equal, we need to check how equal 
            // they are against the bias option.
            var percentage = await hashComparer.CalculatePercentage(
                source.Hash,
                target.Hash);
            if (percentage >= comparisonOptions.BiasPercent)
                return new Some<DuplicateImage>(new DuplicateImage(target, percentage));

            return new None<DuplicateImage>();
        }

        /// <summary>
        /// If the images have the same hash, add them to the duplicate collection.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="duplicateImages"></param>
        /// <returns>`true` if the item was added, and `false` if not.</returns>
        protected async Task<bool> UpdateDuplicationCollection(
            LocalImage source,
            LocalImage target,
            List<DuplicateImage> duplicateImages)
        {
            var duplicate = await DirectComparison(source, target);
            if (duplicate.IsNone) 
                return false;
            
            duplicateImages.Add(duplicate.Value);

            return true;
        }
        
        /// <summary>
        /// Each comparer instance must decide its own method of creating the file hash,
        /// either because they are using a different hash provider, or because they decide
        /// to roll their own method. There is no default implementation, because creating
        /// the hash can potentially be very expensive (on time and memory). Forcing each
        /// implementation to implement this method, prevents them from accidentally using
        /// a hash-ing method that is undesirable.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected abstract Task<string> GetFileHash(string filePath);

        public abstract Action PrintInstructions();
    }
}

using System;
using ImgDiff.Comparers;
using ImgDiff.Comparers.ForBitmap;
using ImgDiff.Comparers.ForImages;
using ImgDiff.Comparers.ForStrings;
using ImgDiff.Hashing;
using ImgDiff.Interfaces;
using ImgDiff.Models;

namespace ImgDiff.Factories
{
    public class ImageComparisonFactory
    {
        /// <summary>
        /// Construct the comparison object that we'll be using for this current run. Once
        /// we have it, we need only call the `Run` method on the result.
        /// </summary>
        /// <param name="fromRequest"><see cref="ComparisonRequest"/>: The representation of the current request.</param>
        /// <param name="withOptions"><see cref="ComparisonOptions"/>: The options that will be used for the request.</param>
        /// <returns>The comparison object, that will handle the current request.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if we get a <see cref="ComparisonWith"/>
        /// value that isn't recognized.</exception>
        public ICompareImages Construct(ComparisonRequest fromRequest, ComparisonOptions withOptions)
        {
            ICompareImages imageComparer;

            switch (fromRequest.With)
            {
                case ComparisonWith.All:
                    imageComparer = new DirectoryComparison(
                        new BasicHashProvider(),
                        new HashComparison(withOptions.BiasPercent), 
                        withOptions);
                    break;
                case ComparisonWith.Pair:
                    imageComparer = new PairComparison(
                        new BasicHashProvider(), 
                        new PixelComparison(withOptions),
                        withOptions);
                    break;
                case ComparisonWith.Single:
                    imageComparer = new SingleComparison(
                        new BasicHashProvider(), 
                        new HashComparison(withOptions.BiasPercent),
                        withOptions);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return imageComparer;
        }
    }
}
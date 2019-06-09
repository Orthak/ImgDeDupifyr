using ImgDiff.Models;

namespace ImgDiff.Interfaces
{
    public abstract class ImageComparer
    {
        protected readonly IHashProvider hashProvider;
        protected readonly ICompareStrings hashComparer;
        protected readonly ComparisonOptions comparisonOptions;

        protected ImageComparer(
            IHashProvider provider,
            ICompareStrings stringComparer,
            ComparisonOptions options)
        {
            hashProvider = provider;
            hashComparer = stringComparer;
            comparisonOptions = options;
        }
    }
}

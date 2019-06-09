using ImgDiff.Exceptions;
using ImgDiff.Models;

namespace ImgDiff.Builders
{
    public class ComparisonOptionsBuilder
    {
        bool? topDirectoryOnly;
        double? biasPercent;

        public ComparisonOptionsBuilder SearchOnlyTopDirectory(bool topOnly)
        {
            topDirectoryOnly = topOnly;

            return this;
        }

        public ComparisonOptionsBuilder ShouldSucceedWithPercentage(double bias)
        {
            biasPercent = bias;

            return this;
        }

        public ComparisonOptions Build()
        {
            if (!topDirectoryOnly.HasValue)
                throw BuildIncompleteException(nameof(topDirectoryOnly));

            if (!biasPercent.HasValue)
                throw BuildIncompleteException(nameof(biasPercent));
            
            return new ComparisonOptions(
                topDirectoryOnly.Value,
                biasPercent.Value);
        }

        IncompleteComparisonOptionsException BuildIncompleteException(string propertyName)
        {
            return new IncompleteComparisonOptionsException(
                $"Comparison Option '{propertyName}' must have a value.");
        }
    }
}

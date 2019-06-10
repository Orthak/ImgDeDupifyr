using ImgDiff.Models;
using ImgDiff.Monads;

namespace ImgDiff.Builders
{
    public class ComparisonRequestBuilder
    {
        Option<string> requestedDirectory   = new None<string>();
        Option<string> requestedFirstImage  = new None<string>();
        Option<string> requestedSecondImage = new None<string>();
        ComparisonWith requestedComparisonWith;

        public ComparisonRequestBuilder InDirectory(string directory)
        {
            requestedDirectory = new Some<string>(directory);

            return this;
        }

        public ComparisonRequestBuilder WithFirstImage(string first)
        {
            requestedFirstImage = new Some<string>(first);

            return this;
        }

        public ComparisonRequestBuilder WithSecondImage(string second)
        {
            requestedSecondImage = new Some<string>(second);

            return this;
        }

        public ComparisonRequestBuilder UsingComparisonAs(ComparisonWith comparisonWith)
        {
            requestedComparisonWith = comparisonWith;

            return this;
        }

        public ComparisonRequest Build()
        {
            return new ComparisonRequest(
                requestedDirectory,
                requestedFirstImage,
                requestedSecondImage,
                requestedComparisonWith);
        }
    }
}
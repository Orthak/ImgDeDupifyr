using System.IO;
using ImgDiff.Constants;
using ImgDiff.Exceptions;

namespace ImgDiff.Models
{
    public class ComparisonOptions
    {
        public SearchOption DirectorySearchOption { get; }
        public Strictness ColorStrictness { get; }
        public double BiasPercent { get; }

        public ComparisonOptions(
            SearchOption directorySearchOption,
            Strictness colorStrictness,
            double biasPercent)
        {
            DirectorySearchOption = directorySearchOption;

            ColorStrictness = colorStrictness;
            
            if (biasPercent > 100)
                throw BuildBiasBoundsException(biasPercent, "greater than 100");
            
            if (biasPercent < 0)
                throw BuildBiasBoundsException(biasPercent, "less than 0");
            
            BiasPercent = biasPercent / 100;
        }
        
        static BiasOutOfBoundsException BuildBiasBoundsException(double value, string boundaryMessage)
        {
            return new BiasOutOfBoundsException($"The given bias value ({value}) cannot be {boundaryMessage}.");
        }
    }
}

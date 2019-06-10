using System.IO;
using ImgDiff.Exceptions;

namespace ImgDiff.Models
{
    public class ComparisonOptions
    {
        public SearchOption DirectorySearchOption { get; }
        public double BiasPercent { get; }

        public ComparisonOptions(
            SearchOption directorySearchOption,
            double biasPercent)
        {
            DirectorySearchOption = directorySearchOption;
            
            if (biasPercent > 100)
                throw BuildBiasBoundsException(biasPercent, "greater than 100");
            
            if (biasPercent < 0)
                throw BuildBiasBoundsException(biasPercent, "less than 0");
            
            BiasPercent = 1/biasPercent;
        }
        
        static BiasOutOfBoundsException BuildBiasBoundsException(double value, string boundaryMessage)
        {
            return new BiasOutOfBoundsException($"The given bias value ({value}) cannot be {boundaryMessage}.");
        }
    }
}

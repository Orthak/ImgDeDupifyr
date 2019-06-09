namespace ImgDiff.Models
{
    public class ComparisonOptions
    {
        public bool TopDirectoryOnly { get; }
        public double BiasPercent { get; }

        public ComparisonOptions(
            bool topDirectoryOnly,
            double biasPercent)
        {
            TopDirectoryOnly = topDirectoryOnly;
            BiasPercent = biasPercent;
        }
    }
}
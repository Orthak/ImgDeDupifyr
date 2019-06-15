using System.Threading.Tasks;
using ImgDiff.Interfaces;

namespace ImgDiff.Comparers.ForStrings
{
    public class HashComparison : ICalculateDifference<string>
    {
        readonly double biasFactor;
        
        public HashComparison(double withBias)
        {
            biasFactor = withBias;
        }
        
        public Task<double> CalculatePercentage(string source, string target)
        {
            // If the lengths aren't equal, don't bother checking. The strings
            // can never be equivalent.
            if (source.Length != target.Length)
                return Task.FromResult(0.0);

            return Task.Run(() =>
            {
                // Keep a running total of the equality, as we check the characters
                // of each string one at a time. We do this so we can break if the 
                // percentage drops below the bias, before we finish the comparison.
                var runningPercentage = 1.0;
                for (var index = 0; index < source.Length; index++)
                {
                    if (source[index] != target[index])
                        runningPercentage -= 1 / (double)source.Length;
                
                    if (runningPercentage < biasFactor)
                        break;
                }

                return runningPercentage;
            });
        }
    }
}

using System.Threading.Tasks;

namespace ImgDiff.Interfaces
{
    public interface ICompareStrings
    {
        Task<double> CalculatePercentage(string source, string target);
    }
}

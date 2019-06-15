using System.Threading.Tasks;

namespace ImgDiff.Interfaces
{
    public interface ICalculateDifference<in T>
    {
        Task<double> CalculatePercentage(T source, T target);
    }
}

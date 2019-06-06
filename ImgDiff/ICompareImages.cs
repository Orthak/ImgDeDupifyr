using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImgDiff
{
    public interface ICompareImages
    {
        Task<List<DuplicateResult>> Run(string request);
    }
}
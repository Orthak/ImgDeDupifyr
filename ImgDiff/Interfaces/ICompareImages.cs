using System.Collections.Generic;
using System.Threading.Tasks;
using ImgDiff.Models;

namespace ImgDiff.Interfaces
{
    public interface ICompareImages
    {
        Task<List<DuplicateResult>> Run(string request);
    }
}

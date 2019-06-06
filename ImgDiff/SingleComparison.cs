using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImgDiff
{
    public class SingleComparison : ICompareImages
    {
        public Task<List<DuplicateResult>> Run(string request)
        {
            
            return Task.FromResult(new List<DuplicateResult>());
        }
    }
}
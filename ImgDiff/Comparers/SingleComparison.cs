using System.Collections.Generic;
using System.Threading.Tasks;
using ImgDiff.Hashing;
using ImgDiff.Interfaces;
using ImgDiff.Models;

namespace ImgDiff.Comparers
{
    public class SingleComparison : ImageComparer, ICompareImages
    {
        public SingleComparison(
            IHashProvider injectedHashProvider,
            ICompareStrings stringComparer,
            ComparisonOptions options)
        : base(injectedHashProvider, stringComparer, options)
        { }
        
        public Task<List<DuplicateResult>> Run(string request)
        {
            
            
            return Task.FromResult(new List<DuplicateResult>());
        }
    }
}

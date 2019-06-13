using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImgDiff.Models;

namespace ImgDiff.Interfaces
{
    public interface ICompareImages
    {
        Task<List<DeDupifyrResult>> Run(ComparisonRequest request);
        Action PrintInstructions();
    }
}

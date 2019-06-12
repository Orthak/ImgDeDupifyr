using System;
using System.Collections.Generic;
using ImgDiff.Monads;

namespace ImgDiff.Models
{
    public class ExecutionStatus
    {
        public ComparisonRequest OriginalRequest { get; }
        
        public bool IsTerminated { get; }
        
        public bool IsFaulted { get; }
        
        public Option<List<DeDupifyrResult>> Results { get; }
        
        public Option<ComparisonOptions> OptionOverrides { get; }

        public Option<Exception> FaultedReason { get; }

        public ExecutionStatus(
            ComparisonRequest originalRequest,
            bool isTerminated,
            bool isFaulted,
            Option<List<DeDupifyrResult>> results,
            Option<ComparisonOptions> optionOverrides,
            Option<Exception> faultedReason)
        {
            OriginalRequest = originalRequest;
            IsTerminated = isTerminated;
            IsFaulted = isFaulted;
            Results = results;
            OptionOverrides = optionOverrides;
            FaultedReason = faultedReason;
        }
    }
}

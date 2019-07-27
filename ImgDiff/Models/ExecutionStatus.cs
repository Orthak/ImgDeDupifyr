using System;
using System.Collections.Generic;
using ImgDiff.Monads;

namespace ImgDiff.Models
{
    // TODO: Create sub-classes for this, for each execution type.
    public class ExecutionStatus
    {
        public ComparisonRequest OriginalRequest { get; }
        
        public bool IsTerminated { get; }
        
        public bool IsFaulted { get; }
        
        public Action PrintingInstructions { get; }

        public Option<List<DeDupifyrResult>> Results { get; }
        
        public Option<ComparisonOptions> OptionOverrides { get; }
        
        public Option<Exception> FaultedReason { get; }

        public ExecutionStatus(
            ComparisonRequest originalRequest,
            bool isTerminated,
            bool isFaulted,
            Action printingInstructions,
            Option<List<DeDupifyrResult>> results,
            Option<ComparisonOptions> optionOverrides,
            Option<Exception> faultedReason)
        {
            OriginalRequest      = originalRequest;
            IsTerminated         = isTerminated;
            IsFaulted            = isFaulted;
            PrintingInstructions = printingInstructions;
            Results              = results;
            OptionOverrides      = optionOverrides;
            FaultedReason        = faultedReason;
        }
    }
}

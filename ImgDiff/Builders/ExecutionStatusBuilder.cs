using System;
using System.Collections.Generic;
using ImgDiff.Models;
using ImgDiff.Monads;

namespace ImgDiff.Builders
{
    public class ExecutionStatusBuilder
    {
        ComparisonRequest originalRequest;
        bool isTerminated;
        bool isFaulted;
        Action instructions;
        Option<List<DeDupifyrResult>> results     = new None<List<DeDupifyrResult>>();
        Option<ComparisonOptions> optionOverrides = new None<ComparisonOptions>();
        Option<Exception> faultedReason           = new None<Exception>();

        public ExecutionStatusBuilder WithOriginalRequest(ComparisonRequest request)
        {
            originalRequest = request;

            return this;
        }

        public ExecutionStatusBuilder ShouldTerminate(bool terminated)
        {
            isTerminated = terminated;

            return this;
        }

        public ExecutionStatusBuilder HasFaulted(bool faulted)
        {
            isFaulted = faulted;

            return this;
        }

        public ExecutionStatusBuilder HasResults(Option<List<DeDupifyrResult>> duplicates)
        {
            results = duplicates;

            return this;
        }

        public ExecutionStatusBuilder OverridesOptionsWith(ComparisonOptions newOptions)
        {
            optionOverrides = new Some<ComparisonOptions>(newOptions);

            return this;
        }

        public ExecutionStatusBuilder WithPrintInstructions(Action printInstructions)
        {
            instructions = printInstructions;

            return this;
        }
        
        public ExecutionStatusBuilder FaultedWithException(Exception faultReason)
        {
            faultedReason = new Some<Exception>(faultReason);

            return this;
        }

        public ExecutionStatus Build()
        {
            return new ExecutionStatus(
                originalRequest,
                isTerminated,
                isFaulted,
                instructions,
                results,
                optionOverrides,
                faultedReason);
        }
    }
}
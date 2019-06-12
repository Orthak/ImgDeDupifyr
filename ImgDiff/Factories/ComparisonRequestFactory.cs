using System.IO;
using ImgDiff.Builders;
using ImgDiff.Models;

namespace ImgDiff.Factories
{
    public class ComparisonRequestFactory
    {
        readonly ComparisonRequestBuilder requestBuilder = new ComparisonRequestBuilder();
        
        /// <summary>
        /// To help better represent what the user requested, and perform better checks
        /// against the request, we build a new object to hold this data.
        /// </summary>
        /// <param name="fromRequest">The raw string request that was given by the user.</param>
        /// <returns>The object representation of the user's request.</returns>
        public ComparisonRequest Construct(string fromRequest)
        {
            if (fromRequest.Contains(','))
            {
                var requestArgs = fromRequest.Split(',');
                if (Directory.Exists(requestArgs[1]))
                {
                    requestBuilder.UsingComparisonAs(ComparisonWith.Single)
                        .InDirectory(requestArgs[1])
                        .WithFirstImage(requestArgs[0]);
                }
                else
                {
                    requestBuilder.UsingComparisonAs(ComparisonWith.Pair)
                        .WithFirstImage(requestArgs[0])
                        .WithSecondImage(requestArgs[1]);
                }
            }
            else
            {
                requestBuilder.UsingComparisonAs(ComparisonWith.All)
                    .InDirectory(fromRequest);
            }

            var comparisonRequest = requestBuilder.Build().Validate();
            
            return comparisonRequest;
        }
    }
}
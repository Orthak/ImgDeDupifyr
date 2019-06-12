using System.Collections.Generic;
using System.Linq;

namespace ImgDiff.Extensions
{
    public static class StringCollectionExtensions
    {
        public static string ToAggregatedString(this IEnumerable<string> toAggregate)
        {
            return toAggregate.Aggregate((total, next) => $"{total}, {next}");
        }
    }
}
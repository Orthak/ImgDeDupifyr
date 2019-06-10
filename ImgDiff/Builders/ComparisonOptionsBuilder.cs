using System;
using System.Collections.Generic;
using System.IO;
using ImgDiff.Constants;
using ImgDiff.Exceptions;
using ImgDiff.Models;
using ImgDiff.Monads;

namespace ImgDiff.Builders
{
    public class ComparisonOptionsBuilder
    {
        SearchOption? searchOption;
        double? biasPercent;

        public ComparisonOptionsBuilder WithSearchOption(SearchOption option)
        {
            searchOption = option;

            return this;
        }

        public ComparisonOptionsBuilder ShouldSucceedWithPercentage(double bias)
        {
            biasPercent = bias;

            return this;
        }

        /// <summary>
        /// Builds the <see cref="ComparisonOptions"/> object, using the values that
        /// the builder has been given.
        /// </summary>
        /// <returns>The newly created options.</returns>
        /// <exception cref="IncompleteComparisonOptionsException"></exception>
        public ComparisonOptions Build()
        {
            return BuildInternal();
        }

        /// <summary>
        /// Here, we build the <see cref="ComparisonOptions"/> object, which will hold all of the options
        /// we want the program to run with. We first take 
        /// </summary>
        /// <param name="flags">The name/value store of flags that was returned by the command parser.</param>
        /// <exception cref="BiasOutOfBoundsException">Thrown if the parsed bias factor
        /// is greater than 100, or less than 0.</exception>
        public ComparisonOptions FromCommandFlags(Dictionary<string, string> flags, Option<ComparisonOptions> currentOptions)
        {
            // If there are no flags, use the default values. Or, if we have some
            // options already defined, use those.
            if (flags.Count <= 0)
            {
                if (currentOptions.IsSome)
                {
                    searchOption = currentOptions.Value.DirectorySearchOption;
                    biasPercent = currentOptions.Value.BiasPercent;
                }

                return BuildInternal();
            }
            
            var directoryLevel = flags[CommandFlagProperties.SearchOptionFlag.Name];
            if (!string.IsNullOrEmpty(directoryLevel))
            {
                if (!Enum.TryParse<SearchOption>(directoryLevel, out var searchIn))
                    searchIn = directoryLevel.Contains("top")
                        ? SearchOption.TopDirectoryOnly
                        : SearchOption.AllDirectories;
                
                searchOption = searchIn;
            }

            var biasFactor = flags[CommandFlagProperties.BiasFactorFlag.Name];
            if (!string.IsNullOrEmpty(biasFactor))
                biasPercent = Convert.ToDouble(biasFactor);
            
            return BuildInternal();
        }

        /// <summary>
        /// Centralize the value checks to this internal method. We don't want to expose
        /// this to outside methods, since they don't need to know, or care, about how
        /// these are validated. They only need to know that it does.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IncompleteComparisonOptionsException"></exception>
        ComparisonOptions BuildInternal()
        {
            if (!searchOption.HasValue)
                searchOption = SearchOption.TopDirectoryOnly;

            if (!biasPercent.HasValue)
                biasPercent = 1/(double)9;
            
            return new ComparisonOptions(
                searchOption.Value,
                biasPercent.Value);
        }
    }
}

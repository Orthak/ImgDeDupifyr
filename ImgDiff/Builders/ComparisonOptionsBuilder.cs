using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImgDiff.Constants;
using ImgDiff.Exceptions;
using ImgDiff.Models;
using ImgDiff.Monads;

namespace ImgDiff.Builders
{
    public class ComparisonOptionsBuilder
    {
        Option<SearchOption> searchOption = new None<SearchOption>();
        Option<Strictness> colorStrictness = new None<Strictness>();
        Option<double> biasPercent = new None<double>();

        public ComparisonOptionsBuilder WithSearchOption(SearchOption option)
        {
            searchOption = new Some<SearchOption>(option);

            return this;
        }

        public ComparisonOptionsBuilder AsStrictAs(Strictness strictness)
        {
            colorStrictness = new Some<Strictness>(strictness);

            return this;
        }
        
        public ComparisonOptionsBuilder ShouldSucceedWithPercentage(double bias)
        {
            biasPercent = new Some<double>(bias);

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
        public ComparisonOptions BuildFromFlags(Dictionary<string, string> flags, Option<ComparisonOptions> currentOptions)
        {
            // If there are no flags, use the default values. Or, if we have some
            // options already defined, use those.
            if (flags.Any() == false)
            {
                if (currentOptions.IsSome)
                {
                    searchOption    = new Some<SearchOption>(currentOptions.Value.DirectorySearchOption);
                    colorStrictness = new Some<Strictness>(currentOptions.Value.ColorStrictness);
                    biasPercent     = new Some<double>(currentOptions.Value.BiasPercent);
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
                
                searchOption = new Some<SearchOption>(searchIn);
            }

            var strictness = flags[CommandFlagProperties.StrictnessFlag.Name];
            if (!string.IsNullOrEmpty(strictness))
                colorStrictness = new Some<Strictness>(Enum.Parse<Strictness>(strictness));

            var biasFactor = flags[CommandFlagProperties.BiasFactorFlag.Name];
            if (!string.IsNullOrEmpty(biasFactor))
                biasPercent = new Some<double>(Convert.ToDouble(biasFactor));
            
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
            if (searchOption.IsNone)
                searchOption = new Some<SearchOption>(SearchOption.TopDirectoryOnly);

            if (colorStrictness.IsNone)
                colorStrictness = new Some<Strictness>(Strictness.Fuzzy);
            
            if (biasPercent.IsNone)
                biasPercent = new Some<double>(1/(double)8);
            
            return new ComparisonOptions(
                searchOption.Value,
                colorStrictness.Value,
                biasPercent.Value);
        }
    }
}

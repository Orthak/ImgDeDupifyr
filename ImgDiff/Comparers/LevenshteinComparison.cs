using System;
using ImgDiff.Interfaces;

namespace ImgDiff.Comparers
{
    public class LevenshteinComparison : ICompareStrings
    {
        /// <summary>
        /// Used to get the percentage of the difference between 2 strings.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns>Decimal value representing the difference between the
        /// source and target strings.</returns>
        public double CalculatePercentage(string source, string target)
        {
            if (string.IsNullOrEmpty(source)
                || string.IsNullOrEmpty(target))
                return 0.0;

            if (source == target)
                return 1.0;

            int stepsToSame = ComputeDistance(source, target);

            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
        
        /// <summary>
        /// Computes the Levenshtein distance between the source and
        /// target strings.
        /// More information can be found at these 2 links:
        /// 1) https://social.technet.microsoft.com/wiki/contents/articles/26805.c-calculating-percentage-similarity-of-2-strings.aspx
        /// 2) http://www.let.rug.nl/~kleiweg/lev/levenshtein.html
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns>The least costly number of transforms required
        /// for the source and target strings to be equal.</returns>
        int ComputeDistance(string source, string target)
        {
            // Validate Parameters
            if (string.IsNullOrEmpty(source)
                || string.IsNullOrEmpty(target))
                return 0;

            // Step 1: The transform count for when either of the strings
            // is 0, is equal to the length of the other. For example, the
            // number of steps to make '' equal to 'hello' would be 5. The
            // reverse scenario is also true.
            if (source == target)
                return source.Length;

            if (source.Length == 0)
                return target.Length;

            if (target.Length == 0)
                return source.Length;
            
            var distance = new int[source.Length + 1, target.Length + 1];
            
            // Step 2: If both string lengths are greater than 0, fill the distance
            // matrix with consecutive integers, based on the lengths of the 2 strings.
            for (var src = 0; src <= source.Length; distance[src, 0] = src++) { };
            for (var trg = 0; trg <= target.Length; distance[0, trg] = trg++) { };

            for (int rowIndex = 1; rowIndex <= source.Length; rowIndex++)
            {
                for (int colIndex = 1; colIndex <= target.Length; colIndex++)
                {
                    // Step 3: Calculate if there is a cost to transform the character 
                    // at the corresponding position in the source and target string.
                    // If they are the same character, there is no cost. If they're different
                    // there is a cost.
                    int cost = (target[colIndex - 1] == source[rowIndex - 1]) 
                        ? 0 
                        : 1;

                    // Step 4: We now look at all the cells from row 1 column 1 and beyond.
                    // These cells represent the actual characters in the strings we'll be
                    // performing the comparison on. To determine the cost, we take the minimum
                    // value, between: The minimum value between the previous distance row
                    // and previous distance column, and the previous distance row and column + 1.
                    // These values represent: deleting the letter indexing the column, inserting
                    // a letter into the row, and replacing the column letter with the row letter. 
                    distance[rowIndex, colIndex] = Math.Min(
                        Math.Min(distance[rowIndex - 1, colIndex] + 1, distance[rowIndex, colIndex - 1] + 1),
                        distance[rowIndex - 1, colIndex - 1] + cost);
                }
            }
            
            // Taking the distance at [source length, target length] gives us the total
            // distance between these two strings. This is because this value represents 
            // the *least costly* number of actions to get the target string from the source.
            return distance[source.Length, target.Length];
        }
    }
}

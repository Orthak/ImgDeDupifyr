using System.Collections.Generic;
using System.Threading.Tasks;
using ImgDiff.Models;

namespace ImgDiff.Interfaces
{
    public interface IParseFlags
    {
        /// <summary>
        /// Add to the list of flags that we should parse.
        /// </summary>
        /// <param name="optionName">The name of the option, so we can save/get it by the name.</param>
        /// <param name="shortName">The string that is used for the short command.</param>
        /// <param name="longName">The string that is used for the long command.</param>
        IParseFlags AddFlag(string optionName, string shortName, string longName);

        IParseFlags AddFlag(CommandFlag commandFlag);

        /// <summary>
        /// In some cases, we need to parse out the string of commands tha the user
        /// gives us. We take in all the commands, with their parameters, as a single
        /// string. This way we can use a regular expression (built from adding the flags)
        /// to parse out each of the flags we're given, along with their values.
        /// </summary>
        /// <param name="commandLineString">The string of commands the user gave us.</param>
        /// <returns>The dictionary of parsed flags, with names and their values.</returns>
        Task<Dictionary<string, string>> Parse(string commandLineString);
        
        /// <summary>
        /// We need this to combine the args array into a single string, so that we
        /// can parse them all properly. This is so that we don't need to do a ton
        /// of comparisons and checks on an array, but just use a regular expression
        /// on a single string. This combined string is then passed to the `string`
        /// override.
        /// </summary>
        /// <param name="commandLineArgs">The string array parameter from the `Main` method.</param>
        /// <returns></returns>
        Task<Dictionary<string, string>> Parse(string[] commandLineArgs);
    }
}
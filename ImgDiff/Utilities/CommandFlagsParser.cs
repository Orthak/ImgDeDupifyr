using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgDiff.Extensions;
using ImgDiff.Interfaces;
using ImgDiff.Models;

namespace ImgDiff.Utilities
{
    public class CommandFlagsParser : IParseFlags
    {
        readonly List<CommandFlag> toParse = new List<CommandFlag>();

        public IParseFlags AddFlag(string optionName, string shortName, string longName)
        {
            toParse.Add(
                new CommandFlag(
                    optionName,
                    shortName,
                    longName
                ));

            return this;
        }

        public IParseFlags AddFlag(CommandFlag commandFlag)
        {
            toParse.Add(commandFlag);

            return this;
        }

        public Task<Dictionary<string, string>> Parse(string commandLineString) =>
            Task.Run(() =>
            {
                var validFlags = new Dictionary<string, string>();
                for (int index = 0; index < toParse.Count; index++)
                {
                    var flagRegex = new Regex(toParse[index].Regex,
                        RegexOptions.IgnoreCase & RegexOptions.CultureInvariant);
                    var match = flagRegex.Match(commandLineString);

                    if (match.Success)
                        validFlags[toParse[index].Name] = match.Groups[toParse[index].Name].Value;
                }

                return validFlags;
            });

        public Task<Dictionary<string, string>> Parse(string[] commandLineArgs)
        {
            // If we don't get any args, then there's nothing to parse. 
            // Return back an empty dictionary, so we can use the
            // default values.
            if (commandLineArgs.Any() == false)
            {
                return Task.FromResult(new Dictionary<string, string>());
            }

            var flagsString = commandLineArgs.ToAggregatedString();

            return Parse(flagsString);
        }
    }
}

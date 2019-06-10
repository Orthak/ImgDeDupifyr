using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgDiff.Interfaces;
using ImgDiff.Models;

namespace ImgDiff.Utilities
{
    public class CommandFlagsParser : IParseFlags
    {
        readonly List<CommandFlag> toParse = new List<CommandFlag>();
        
        public void AddFlag(string optionName, string shortName, string longName) =>
            toParse.Add(
                new CommandFlag(
                    optionName,
                    shortName,
                    longName
                ));

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
            var flagsString = commandLineArgs.Aggregate((total, next) => $"{total} {next}");

            return Parse(flagsString);
        }
    }
}

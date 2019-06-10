namespace ImgDiff.Models
{
    public class CommandFlag
    {
        public string Name { get; }
        public string ShortString { get; }
        public string LongString { get; }
        public string Regex { get; }

        public CommandFlag(
            string name,
            string shortString,
            string longString)
        {
            Name = name;
            ShortString = shortString;
            LongString = longString;
            
            Regex = $"(-{ShortString}|--{LongString})\\s?" 
                    + $"(?<{Name}>\\w+)\\s?";
        }
    }
}
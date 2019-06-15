using ImgDiff.Models;

namespace ImgDiff.Constants
{
    public static class CommandFlagProperties
    {
        public static readonly CommandFlag SearchOptionFlag = new CommandFlag("DirectoryLevel", "l", "level");
        public static readonly CommandFlag BiasFactorFlag   = new CommandFlag("BiasFactor", "b", "bias");
        public static readonly CommandFlag StrictnessFlag   = new CommandFlag("Strictness", "s", "strictness");
    }
}
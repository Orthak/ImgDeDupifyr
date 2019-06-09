namespace ImgDiff.Models
{
    public class LocalImage
    {
        public string Name { get; }
        public string FilePath { get; }
        public string Extension { get; }
        public string Hash { get; }

        public LocalImage(
            string name,
            string filePath,
            string extension,
            string hash)
        {
            Name = name;
            FilePath = filePath;
            Extension = extension;
            Hash = hash;
        }
    }
}

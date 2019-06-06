namespace ImgDiff
{
    public class LocalImage
    {
        public string Name { get; }
        public string Extension { get; }
        public string Hash { get; }

        public LocalImage(
            string name,
            string extension,
            string hash)
        {
            Name = name;
            Extension = extension;
            Hash = hash;
        }
    }
}

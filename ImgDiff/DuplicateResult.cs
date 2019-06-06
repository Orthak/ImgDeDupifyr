using System.Collections.Generic;

namespace ImgDiff
{
    public class DuplicateResult
    {
        public LocalImage BaseImage { get; }

        public List<LocalImage> Duplicates { get; }

        public DuplicateResult(LocalImage baseImage)
        {
            BaseImage = baseImage;
            Duplicates = new List<LocalImage>();
        }
    }
}

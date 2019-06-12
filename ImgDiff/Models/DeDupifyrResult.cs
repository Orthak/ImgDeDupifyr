using System.Collections.Generic;

namespace ImgDiff.Models
{
    public class DeDupifyrResult
    {
        public LocalImage SourceImage { get; }

        public List<DuplicateImage> Duplicates { get; }

        public DeDupifyrResult(LocalImage baseImage)
        {
            SourceImage = baseImage;
            Duplicates = new List<DuplicateImage>();
        }
    }
}

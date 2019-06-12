namespace ImgDiff.Models
{
    public class DuplicateImage
    {
        public LocalImage Image { get; }
        public double DuplicationPercent { get; }

        public DuplicateImage(LocalImage image, double percent)
        {
            Image = image;
            DuplicationPercent = percent;
        }
    }
}
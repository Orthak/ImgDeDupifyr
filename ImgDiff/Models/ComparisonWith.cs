namespace ImgDiff.Models
{
    public enum ComparisonWith
    {
        /// <summary>
        /// Set to compare all images in a directory with one another.
        /// </summary>
        All,
        
        /// <summary>
        /// Set that only 2 specific images will be compared.
        /// </summary>
        Pair,
        
        /// <summary>
        /// Set that a single image will be compared against a directory.
        /// </summary>
        Single
    }
}
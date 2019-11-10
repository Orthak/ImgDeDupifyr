namespace ImgDiff.Constants
{
    /// <summary>
    /// Used to tell the pixel comparer how far it can reach, to
    /// determine if 2 pixel can be considered the same.
    /// </summary>
    public enum Strictness
    {
        // They both must be the same value.
        Equal = 0,
        
        // Uses a small range.
        Fuzzy = 5,
        
        // Uses a wide range.
        Loose = 15
    }
}

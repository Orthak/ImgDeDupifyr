namespace ImgDiff.Monads
{
    public class None<T> : Option<T>
    {
        public None()
        : base(default, false, true)
        { }
    }
}
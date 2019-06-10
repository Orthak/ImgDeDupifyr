namespace ImgDiff.Monads
{
    public class Some<T> : Option<T>
    {
        public Some(T value)
        : base(value, true, false)
        { }
    }
}

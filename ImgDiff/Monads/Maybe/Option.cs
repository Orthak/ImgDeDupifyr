namespace ImgDiff.Monads
{
    public abstract class Option<T>
    {
        public T Value     { get; }
        public bool IsSome { get; }
        public bool IsNone { get; }

        protected Option(T value, bool isSome, bool isNone)
        {
            Value = value;
            IsSome = isSome;
            IsNone = isNone;
        }
    }
}

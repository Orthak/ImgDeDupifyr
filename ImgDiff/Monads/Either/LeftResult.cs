namespace ImgDiff.Monads.Either
{
    public class LeftResult<TLeft, TRight> : Result<TLeft, TRight>
    {
        public LeftResult(TLeft leftValue) 
        : base(leftValue, true, default, false)
        { }
    }
}

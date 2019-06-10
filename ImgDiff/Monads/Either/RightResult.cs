namespace ImgDiff.Monads.Either
{
    public class RightResult<TLeft, TRight> : Result<TLeft, TRight>
    {
        public RightResult(TRight rightValue)
        : base(default, false, rightValue, true) 
        {}
    }
}

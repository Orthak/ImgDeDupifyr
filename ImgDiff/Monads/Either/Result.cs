namespace ImgDiff.Monads.Either
{
    public abstract class Result<TLeft, TRight>
    {
        public TLeft Left { get; }
        public TRight Right { get; }
        
        public bool IsLeft { get; }
        public bool IsRight { get; }
        
        protected Result(TLeft leftValue, bool isLeft,
                         TRight rightValue, bool isRight)
        {
            Left = leftValue;
            IsLeft = isLeft;
            Right = rightValue;
            IsRight = isRight;
        }
    }
}

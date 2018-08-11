
namespace Kontur
{
    public class Result<TResult, TError>
    {
        public Result(TResult result)
        {
            this.Value = result;
            this.Success = true;
        }

        public Result(TError error)
        {
            this.Error = error;
            this.Success = false;
        }

        public bool Success { get; }
        public TResult Value { get; }
        public TError Error { get; }
    }
}

using System.Runtime.ExceptionServices;

public class Result<T>
{
    public Result(T result)
    {
        this.Value = result;
        this.Success = true;
    }

    public Result(ExceptionDispatchInfo error)
    {
        this.Error = error;
        this.Success = false;
    }

    public bool Success { get; }
    public T Value { get; }
    public ExceptionDispatchInfo Error { get; }
}

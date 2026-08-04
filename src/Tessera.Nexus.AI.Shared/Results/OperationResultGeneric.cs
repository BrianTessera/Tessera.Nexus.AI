namespace Tessera.Nexus.AI.Shared.Results;

public sealed class OperationResult<T>
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public T? Value { get; init; }

    public static OperationResult<T> Ok(T value, string? message = null)
    {
        return new OperationResult<T>
        {
            Success = true,
            Value = value,
            Message = message
        };
    }

    public static OperationResult<T> Fail(string message)
    {
        return new OperationResult<T>
        {
            Success = false,
            Message = message
        };
    }
}

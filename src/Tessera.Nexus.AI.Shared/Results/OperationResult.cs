namespace Tessera.Nexus.AI.Shared.Results;

public sealed class OperationResult
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public static OperationResult Ok(string? message = null)
    {
        return new OperationResult
        {
            Success = true,
            Message = message
        };
    }

    public static OperationResult Fail(string message)
    {
        return new OperationResult
        {
            Success = false,
            Message = message
        };
    }
}

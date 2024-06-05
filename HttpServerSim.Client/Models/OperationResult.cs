namespace HttpServerSim.Client.Models
{
    /// <summary>
    /// Defines the contract for the response used in the control-endpoint
    /// The control-endpoint allows to create a rules and verify how many times a rule was used
    /// </summary>
    public class OperationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public static OperationResult CreateSuccess() => new() { Success = true };
        public static OperationResult<TData> CreateSuccess<TData>(TData data) => new() { Success = true, Data = data };
        public static OperationResult CreateFailure(string message) => new() { Message = message };
    }

    public class OperationResult<TData> : OperationResult
    {
        public TData? Data { get; set; }
    }
}

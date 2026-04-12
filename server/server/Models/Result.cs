namespace server.Models.DTO
{
    /// <summary>
    /// A generic wrapper used across services to standardize operation results.
    /// Encapsulates the success state, the returned data, and potential error messages.
    /// </summary>
    /// <typeparam name="T">Type of the payload data.</typeparam>
    public class Result<T>
    {
        public bool Success { get; }
        public string? Error { get; }
        public T? Data { get; }

        private Result(bool success, T? data = default, string? error = null)
        {
            Success = success;
            Data = data;
            Error = error;
        }

        /// <summary>
        /// Creates a successful result containing the provided data.
        /// </summary>
        public static Result<T> Ok(T data) => new Result<T>(true, data);

        /// <summary>
        /// Creates a failed result with a specific error description.
        /// </summary>
        public static Result<T> Fail(string error) => new Result<T>(false, default, error);
    }
}
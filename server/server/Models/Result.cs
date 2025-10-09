namespace server.Models
{
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

        public static Result<T> Ok(T data) => new Result<T>(true, data);
        public static Result<T> Fail(string error) => new Result<T>(false, default, error);
    }

}

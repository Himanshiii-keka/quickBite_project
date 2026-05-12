namespace startup_project.Models.Common
{
    /// <summary>
    /// Generic result wrapper returned by service layer. Carries an HTTP status code
    /// so controllers can map success/failure to the correct response without inspecting messages.
    /// </summary>
    public record ServiceResult<T>(bool Success, int StatusCode, string Message, T? Data)
    {
        public static ServiceResult<T> Ok(T data, string message = "Success") =>
            new(true, StatusCodes.Status200OK, message, data);

        public static ServiceResult<T> Created(T data, string message = "Created") =>
            new(true, StatusCodes.Status201Created, message, data);

        public static ServiceResult<T> Fail(int statusCode, string message) =>
            new(false, statusCode, message, default);
    }

    /// <summary>Non-generic variant for endpoints that don't return data on success.</summary>
    public record ServiceResult(bool Success, int StatusCode, string Message)
    {
        public static ServiceResult Ok(string message = "Success") =>
            new(true, StatusCodes.Status200OK, message);

        public static ServiceResult Fail(int statusCode, string message) =>
            new(false, statusCode, message);
    }
}

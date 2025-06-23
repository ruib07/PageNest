using PageNest.Application.Shared.DTOs;

namespace PageNest.Application.Shared.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Data { get; }
    public ResponsesDTO.Error Error { get; }
    public string Message { get; }

    private Result(bool isSuccess, T data, ResponsesDTO.Error error, string message = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        Message = message;
    }

    public static Result<T> Success(T data) => new(true, data, null, null);
    public static Result<T> Success(T data, string message) => new(true, data, null, message);
    public static Result<T> Fail(string message, int statusCode) => new(false, default, new ResponsesDTO.Error(message, statusCode));
}

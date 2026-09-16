namespace Nexodus_Back.Application.DTOs.Common;

public class ApiResponse<T>
{
    public bool Successful { get; set; }
    public int Code { get; set; }
    public T? Content { get; set; }

    public static ApiResponse<T> Success(T content, int code = 200)
    {
        return new ApiResponse<T>
        {
            Successful = true,
            Code = code,
            Content = content
        };
    }

    public static ApiResponse<T> Error(T content, int code)
    {
        return new ApiResponse<T>
        {
            Successful = false,
            Code = code,
            Content = content
        };
    }
}

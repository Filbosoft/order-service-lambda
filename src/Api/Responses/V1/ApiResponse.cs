using Microsoft.AspNetCore.Http;

namespace Api.Responses.V1
{
    public class ApiResponse<T>

    {
        public int StatusCode { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public ApiResponse()
        { }

        public ApiResponse(T data)
        {
            StatusCode = StatusCodes.Status200OK;
            Data = data;
            Message = "";
        }
    }
}
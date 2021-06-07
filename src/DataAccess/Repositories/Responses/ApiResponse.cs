using Microsoft.AspNetCore.Http;

namespace DataAccess.Repositories.Responses
{
    public class ApiResponse<T>

    {
        public int Status { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }

        public ApiResponse()
        { }

        public ApiResponse(T data)
        {
            Status = StatusCodes.Status200OK;
            Data = data;
            Message = "";
        }
    }
}
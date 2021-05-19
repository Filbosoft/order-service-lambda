namespace Business
{
    public static class BusinessResponse
    {
        public static BusinessResponse<T> Ok<T>(T data, string message = default) => 
            new BusinessResponse<T>
            {
                Data = data,
                Message = message,
                IsError = false
            };
        public static BusinessResponse<T> Fail<T>(string message, T data = default) => 
            new BusinessResponse<T>
            {
                Data = data,
                Message = message,
                IsError = true
            };
        
    }
    public class BusinessResponse<T>
    {
        public T Data { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
    }
}
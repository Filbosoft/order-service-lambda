using Business;

namespace Api.Responses.V1
{
    public class PagedApiResponse<T> : ApiResponse<T>
    {
        public Pagination Pagination { get; set; }
    }
}
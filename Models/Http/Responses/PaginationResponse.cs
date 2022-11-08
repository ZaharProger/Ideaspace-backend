namespace Ideaspace_backend.Models.Http.Responses
{
    public class PaginationResponse<T> : DataResponse<T>
    {
        public bool IsOver { get; set; }
    }
}

using Ideaspace_backend.Models.Entities;

namespace Ideaspace_backend.Models.Http.Responses
{
    public class DataResponse<T> : BaseResponse
    {
        public T[] Data { get; set; }
    }
}

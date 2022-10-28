using Ideaspace_backend.Models.Entities;

namespace Ideaspace_backend.Models.Http.Responses
{
    public class UserDataResponse : BaseResponse
    {
        public User[] Data { get; set; }
    }
}

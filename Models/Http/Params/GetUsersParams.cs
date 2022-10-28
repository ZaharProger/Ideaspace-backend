namespace Ideaspace_backend.Models.Http.Params
{
    public class GetUsersParams
    {
        public string SessionId { get; set; } = "";
        public string UserId { get; set; } = "";
        public string SearchString { get; set; } = "";
    }
}

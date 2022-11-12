namespace Ideaspace_backend.Models.Http.Params
{
    public class GetUsersParams
    {
        public string UserLogin { get; set; } = "";
        public string SearchString { get; set; } = "";
        public int Limit { get; set; } = 30;
    }
}

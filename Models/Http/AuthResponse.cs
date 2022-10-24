namespace Ideaspace_backend.Models.Http
{
    public class AuthResponse : RegAuthResponse
    {
        public byte[] SessionId { get; set; }
    }
}

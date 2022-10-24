using System.ComponentModel.DataAnnotations;

namespace Ideaspace_backend.Models.Entities
{
    public class Session
    {
        [Key]
        public ulong Id { get; set; }

        public ulong? UserId { get; set; }
    }
}

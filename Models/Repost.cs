using System.ComponentModel.DataAnnotations;

namespace Ideaspace_backend.Models
{
    public class Repost
    {
        [Key]
        public ulong Id { get; set; }

        public ulong? UserId { get; set; }

        public ulong? PostId { get; set; }
    }
}

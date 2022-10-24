using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ideaspace_backend.Models.Entities
{
    public class Repost
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public ulong Id { get; set; }

        public ulong? UserId { get; set; }

        public ulong? PostId { get; set; }
    }
}

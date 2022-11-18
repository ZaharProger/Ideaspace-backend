using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ideaspace_backend.Models.Entities
{
    public class Repost
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key, Column("repost_id")]
        public long RepostId { get; set; }

        [Column("user_id")]
        public long? UserId { get; set; }

        [Column("post_id")]
        public long? PostId { get; set; }
    }
}

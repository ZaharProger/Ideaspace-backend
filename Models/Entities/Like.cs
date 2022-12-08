using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ideaspace_backend.Models.Entities
{
    public class Like
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key, Column("like_id")]
        public long LikeId { get; set; }

        [Column("user_id")]
        public long? UserId { get; set; }

        [Column("post_id")]
        public long? PostId { get; set; }

        [Column("like_date")]
        public long LikeDate { get; set; }

        [Column("like_time")]
        public int LikeTime { get; set; }
    }
}

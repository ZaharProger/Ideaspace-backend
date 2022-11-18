using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ideaspace_backend.Models.Entities
{
    public class Post
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key, Column("post_id")]
        public long PostId { get; set; }

        [Column("user_id")]
        public long? UserId { get; set; }

        [NotMapped]
        public string UserLogin { get; set; }

        [Column("creation_date")]
        public long CreationDate { get; set; }

        [Column("creation_time")]
        public int CreationTime { get; set; }

        [Column("content", TypeName = "varchar(250)")]
        public string Content { get; set; }
    }
}

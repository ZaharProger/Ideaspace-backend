using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ideaspace_backend.Models.Entities
{
    public class Post
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public long post_id { get; set; }

        public long? user_id { get; set; }

        public long creation_date { get; set; }

        public int creation_time { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string content { get; set; }
    }
}

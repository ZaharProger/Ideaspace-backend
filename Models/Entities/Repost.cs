using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ideaspace_backend.Models.Entities
{
    public class Repost
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public long repost_id { get; set; }

        public long? user_id { get; set; }

        public long? post_id { get; set; }
    }
}

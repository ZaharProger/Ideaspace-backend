using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ideaspace_backend.Models
{
    public class Post
    {
        [Key]
        public ulong Id { get; set; }

        public ulong? UserId { get; set; }

        public ulong CreationDate { get; set; }

        public uint CreationTime { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string Content { get; set; }
    }
}

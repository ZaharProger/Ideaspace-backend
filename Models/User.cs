using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ideaspace_backend.Models
{
    public class User
    {
        [Key]
        public ulong Id { get; set; }

        [Column(TypeName = "varchar(30")]
        public string Login { get; set; }

        [Column(TypeName = "varchar(100")]
        public string Password { get; set; }

        public ulong? Birthday { get; set; }

        [Column(TypeName = "varchar(100")]
        public string? Status { get; set; }
    }
}

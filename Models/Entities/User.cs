using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ideaspace_backend.Models.Entities
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public long user_id { get; set; }

        [Column(TypeName = "varchar(30")]
        public string user_login { get; set; }

        [Column(TypeName = "varbinary(100")]
        public byte[] user_password { get; set; }

        public long? user_birthday { get; set; }

        [Column(TypeName = "varchar(100")]
        public string? user_status { get; set; }
    }
}

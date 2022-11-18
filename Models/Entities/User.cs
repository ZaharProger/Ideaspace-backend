using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ideaspace_backend.Models.Entities
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key, Column("user_id")]
        public long UserId { get; set; }

        [Column("user_login", TypeName = "varchar(30")]
        public string UserLogin { get; set; }

        [Column("user_password", TypeName = "varbinary(100")]
        public byte[] UserPassword { get; set; }

        [Column("user_birthday")]
        public long? UserBirthday { get; set; }

        [Column("user_status", TypeName = "varchar(100")]
        public string? UserStatus { get; set; }
    }
}

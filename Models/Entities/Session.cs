using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ideaspace_backend.Models.Entities
{
    public class Session
    {
        [Column("session_id"), Key]
        public long SessionId { get; set; }

        [Column("user_id")]
        public long? UserId { get; set; }
    }
}

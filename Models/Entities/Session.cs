using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ideaspace_backend.Models.Entities
{
    public class Session
    {
        [Column(TypeName = "varbinary(100"), Key]
        public byte[] session_id { get; set; }

        public long? user_id { get; set; }
    }
}

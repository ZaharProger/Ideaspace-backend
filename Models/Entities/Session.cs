using System.ComponentModel.DataAnnotations;

namespace Ideaspace_backend.Models.Entities
{
    public class Session
    {
        [Key]
        public long session_id { get; set; }

        public long? user_id { get; set; }
    }
}

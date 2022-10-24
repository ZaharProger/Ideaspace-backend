using Ideaspace_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ideaspace_backend.Models
{
    public class IdeaspaceDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Repost> Reposts { get; set; }

        public IdeaspaceDBContext(DbContextOptions<IdeaspaceDBContext> options) : base(options)
        { }
    }
}

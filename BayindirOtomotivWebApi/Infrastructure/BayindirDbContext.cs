using Microsoft.EntityFrameworkCore;
using BayindirOtomotivWebApi.Infrastructure;

namespace BayindirOtomotivWebApi.Infrastructure
{
    public class BayindirDbContext : DbContext
    {
        public BayindirDbContext(DbContextOptions<BayindirDbContext> opt) : base(opt) { }

        public DbSet<TokenInfo> IdeaSoftTokens => Set<TokenInfo>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<TokenInfo>(e =>
            {
                e.HasKey(t => t.Id);
                e.Property(t => t.Id)
                 .ValueGeneratedOnAdd();
            });
        }
    }
}

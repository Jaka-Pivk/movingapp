

using API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class StoreContext : IdentityDbContext<User, Role, int>
    {
        public StoreContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<UserEvent> UserEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserEvent>(ue =>
            {
                ue.HasKey(x => new { x.UserId, x.EventId });

                ue.HasOne(x => x.User)
                    .WithMany(u => u.UserEvents)
                    .HasForeignKey(x => x.UserId);

                ue.HasOne(x => x.Event)
                    .WithMany(e => e.UserEvents)
                    .HasForeignKey(x => x.EventId);
            });

            modelBuilder.Entity<Role>()
                .HasData(
                    new Role { Id = 1, Name = "Member", NormalizedName = "MEMBER" },
                    new Role { Id = 2, Name = "Admin", NormalizedName = "ADMIN" }
                );
        }
    }
}

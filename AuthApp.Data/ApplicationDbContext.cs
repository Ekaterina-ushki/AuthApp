using AuthApp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<DashboardTask> DashboardTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<DashboardTask>(TaskConfiguration);
            builder.Entity<User>(UserConfiguration);
        }

        private void UserConfiguration(EntityTypeBuilder<User> builder)
        {
            builder
                .HasMany(x => x.Roles)
                .WithOne()
                .HasForeignKey(x => x.UserId);

            builder
                .HasMany(x => x.Tasks)
                .WithOne()
                .HasForeignKey(x => x.OwnerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void TaskConfiguration(EntityTypeBuilder<DashboardTask> builder)
        {
            builder
                .HasKey(x => x.TaskId);

            builder.Property(x => x.Name)
                .IsRequired();
        }
    }
}

using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Identity.Client;

namespace Infrastructure.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
           
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>().HasKey(k => k.Id);

            builder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithOne(u => u.RefreshToken)
                .HasForeignKey<RefreshToken>(k => k.UserId);

            builder.Entity<RefreshToken>()
                .HasIndex(i => i.Token);
        }

    }
}

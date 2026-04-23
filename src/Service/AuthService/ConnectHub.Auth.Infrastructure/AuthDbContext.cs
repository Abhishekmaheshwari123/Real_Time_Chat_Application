using Microsoft.EntityFrameworkCore;
using ConnectHub.Auth.Domain;

namespace ConnectHub.Auth.Infrastructure;

    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
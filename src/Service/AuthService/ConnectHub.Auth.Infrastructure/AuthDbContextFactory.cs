using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConnectHub.Auth.Infrastructure;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=.;Database=AuthDB;Trusted_Connection=True;TrustServerCertificate=True");

        return new AuthDbContext(optionsBuilder.Options);
    }
}
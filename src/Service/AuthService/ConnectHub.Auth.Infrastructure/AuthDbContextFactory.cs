using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConnectHub.Auth.Infrastructure;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=ep-orange-fog-appzs48p-pooler.c-7.us-east-1.aws.neon.tech; Database=AuthDB; Username=neondb_owner; Password=npg_HU2bfrz0oITe; SSL Mode=VerifyFull; Channel Binding=Require;",
            x => x.MigrationsAssembly("ConnectHub.Auth.API"));

        return new AuthDbContext(optionsBuilder.Options);
    }
}
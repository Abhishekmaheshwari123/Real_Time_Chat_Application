using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ConnectHub.Chat.Infrastructure;

public class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
{
    public ChatDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=ep-orange-fog-appzs48p-pooler.c-7.us-east-1.aws.neon.tech; Database=MediaDB; Username=neondb_owner; Password=npg_HU2bfrz0oITe; SSL Mode=VerifyFull; Channel Binding=Require;",
            x => x.MigrationsAssembly("ConnectHub.Chat.API")
        );

        return new ChatDbContext(optionsBuilder.Options);
    }
}
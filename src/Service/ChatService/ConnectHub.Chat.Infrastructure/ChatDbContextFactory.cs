using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ConnectHub.Chat.Infrastructure;

public class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
{
    public ChatDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=.;Database=ChatDB;Trusted_Connection=True;TrustServerCertificate=True"
        );

        return new ChatDbContext(optionsBuilder.Options);
    }
}
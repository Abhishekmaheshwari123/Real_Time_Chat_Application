using Microsoft.EntityFrameworkCore;
using ConnectHub.Chat.Domain;

namespace ConnectHub.Chat.Infrastructure;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; }
}
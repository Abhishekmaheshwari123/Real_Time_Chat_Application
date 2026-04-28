using Microsoft.EntityFrameworkCore;
using ConnectHub.Notification.Domain.Entities;

namespace ConnectHub.Notification.Infrastructure.Persistence
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        public DbSet<global::ConnectHub.Notification.Domain.Entities.Notification> Notifications { get; set; }
    }
}
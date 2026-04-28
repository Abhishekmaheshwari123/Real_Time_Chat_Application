using ConnectHub.Notification.Application.Interfaces;
using ConnectHub.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConnectHub.Notification.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<List<global::ConnectHub.Notification.Domain.Entities.Notification>> GetByUserIdAsync(string userId)
        {
            return await _context.Notifications
                .Where(x => x.RecipientId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<global::ConnectHub.Notification.Domain.Entities.Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task AddAsync(global::ConnectHub.Notification.Domain.Entities.Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }               

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientId == userId && !n.IsRead);
        }
    }
}
using ConnectHub.Notification.Application.Interfaces;
using ConnectHub.Notification.Domain.Entities;

namespace ConnectHub.Notification.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<global::ConnectHub.Notification.Domain.Entities.Notification>> GetUserNotifications(string userId)
        {
            return await _repository.GetByUserIdAsync(userId);
        }

        public async Task CreateNotification(global::ConnectHub.Notification.Domain.Entities.Notification notification)
        {
            notification.Id = Guid.NewGuid();
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;

            await _repository.AddAsync(notification);
            await _repository.SaveChangesAsync();
        }

        public async Task MarkAsRead(Guid id)
        {
            var notification = await _repository.GetByIdAsync(id);

            if (notification != null)
            {
                notification.IsRead = true;
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnreadCount(string userId)
        {
            return await _repository.GetUnreadCountAsync(userId);
        }
    }
}
using ConnectHub.Notification.Domain.Entities;

namespace ConnectHub.Notification.Application.Interfaces
{
    public interface INotificationService
    {
        Task<List<global::ConnectHub.Notification.Domain.Entities.Notification>> GetUserNotifications(string userId);
        Task CreateNotification(global::ConnectHub.Notification.Domain.Entities.Notification notification);
        Task MarkAsRead(Guid id);
        Task<int> GetUnreadCount(string userId);
    }
}


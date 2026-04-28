using ConnectHub.Notification.Domain.Entities;

namespace ConnectHub.Notification.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<global::ConnectHub.Notification.Domain.Entities.Notification>> GetByUserIdAsync(string userId);
        Task<global::ConnectHub.Notification.Domain.Entities.Notification?> GetByIdAsync(Guid id);
        Task AddAsync(global::ConnectHub.Notification.Domain.Entities.Notification notification);
        Task<int> GetUnreadCountAsync(string userId);
        Task SaveChangesAsync();
    }
}
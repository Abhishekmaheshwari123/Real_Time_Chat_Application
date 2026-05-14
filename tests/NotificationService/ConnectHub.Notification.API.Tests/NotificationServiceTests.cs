using ConnectHub.Notification.Application.Interfaces;
using ConnectHub.Notification.Application.Services;
using ConnectHub.Notification.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ConnectHub.Notification.API.Tests;

[TestClass]
public class NotificationServiceTests
{
    private Mock<INotificationRepository> _mockRepo = null!;
    private NotificationService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepo = new Mock<INotificationRepository>();
        _service = new NotificationService(_mockRepo.Object);
    }

    [TestMethod]
    public async Task GetUserNotifications_CallsRepository()
    {
        var userId = "user1";
        var notifications = new List<global::ConnectHub.Notification.Domain.Entities.Notification> { new() { RecipientId = userId } };
        _mockRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(notifications);

        var result = await _service.GetUserNotifications(userId);

        Assert.AreEqual(notifications, result);
        _mockRepo.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }

    [TestMethod]
    public async Task CreateNotification_SetsDefaultValues_AndCallsRepository()
    {
        var notification = new global::ConnectHub.Notification.Domain.Entities.Notification { RecipientId = "user1", Message = "test" };

        await _service.CreateNotification(notification);

        Assert.AreNotEqual(Guid.Empty, notification.Id);
        Assert.IsFalse(notification.IsRead);
        Assert.IsTrue((DateTime.UtcNow - notification.CreatedAt).TotalSeconds < 5);
        
        _mockRepo.Verify(r => r.AddAsync(notification), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [TestMethod]
    public async Task MarkAsRead_WhenNotificationExists_SetsIsReadAndSaves()
    {
        var id = Guid.NewGuid();
        var notification = new global::ConnectHub.Notification.Domain.Entities.Notification { Id = id, IsRead = false };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(notification);

        await _service.MarkAsRead(id);

        Assert.IsTrue(notification.IsRead);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [TestMethod]
    public async Task MarkAsRead_WhenNotificationDoesNotExist_DoesNotSave()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((global::ConnectHub.Notification.Domain.Entities.Notification?)null);

        await _service.MarkAsRead(id);

        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

}

using ConnectHub.Notification.API.Controllers;
using ConnectHub.Notification.Application.Interfaces;
using ConnectHub.Notification.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConnectHub.Notification.API.Tests;

[TestClass]
public class NotificationControllerTests
{
    [TestMethod]
    public async Task Get_ReturnsNotifications_ForUser()
    {
        var fakeService = new FakeNotificationService
        {
            NotificationsToReturn = new List<global::ConnectHub.Notification.Domain.Entities.Notification>
            {
                new() { Id = Guid.NewGuid(), RecipientId = "u1", Message = "hello", Type = "MESSAGE", CreatedAt = DateTime.UtcNow }
            }
        };
        var controller = new NotificationController(fakeService);

        var result = await controller.Get("u1");

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var payload = okResult.Value as List<global::ConnectHub.Notification.Domain.Entities.Notification>;
        Assert.IsNotNull(payload);
        Assert.AreEqual(1, payload.Count);
        Assert.AreEqual("u1", fakeService.LastGetUserId);
    }

    [TestMethod]
    public async Task Create_CallsService_AndReturnsOk()
    {
        var fakeService = new FakeNotificationService();
        var controller = new NotificationController(fakeService);
        var notification = new global::ConnectHub.Notification.Domain.Entities.Notification
        {
            Id = Guid.NewGuid(),
            RecipientId = "u2",
            Type = "MESSAGE",
            Message = "created",
            CreatedAt = DateTime.UtcNow
        };


        var result = await controller.Create(notification);

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual("Notification created", okResult.Value);
        Assert.AreEqual(notification, fakeService.LastCreatedNotification);
    }

    [TestMethod]
    public async Task MarkAsRead_CallsService_AndReturnsOk()
    {
        var fakeService = new FakeNotificationService();
        var controller = new NotificationController(fakeService);
        var notificationId = Guid.NewGuid();

        var result = await controller.MarkAsRead(notificationId);

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual("Marked as read", okResult.Value);
        Assert.AreEqual(notificationId, fakeService.LastMarkedId);
    }

    private sealed class FakeNotificationService : INotificationService
    {
        public string LastGetUserId { get; private set; } = string.Empty;
        public global::ConnectHub.Notification.Domain.Entities.Notification? LastCreatedNotification { get; private set; }
        public Guid LastMarkedId { get; private set; }
        public List<global::ConnectHub.Notification.Domain.Entities.Notification> NotificationsToReturn { get; set; } = new();

        public Task<List<global::ConnectHub.Notification.Domain.Entities.Notification>> GetUserNotifications(string userId)
        {
            LastGetUserId = userId;
            return Task.FromResult(NotificationsToReturn);
        }

        public Task CreateNotification(global::ConnectHub.Notification.Domain.Entities.Notification notification)
        {
            LastCreatedNotification = notification;
            return Task.CompletedTask;
        }


        public Task MarkAsRead(Guid id)
        {
            LastMarkedId = id;
            return Task.CompletedTask;
        }

        public Task<int> GetUnreadCount(string userId)
        {
            return Task.FromResult(NotificationsToReturn.Count(item => item.RecipientId == userId && !item.IsRead));
        }
    }
}
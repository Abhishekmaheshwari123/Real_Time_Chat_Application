using System.Security.Claims;
using ConnectHub.Notification.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ConnectHub.Notification.API.Tests;

[TestClass]
public class NotificationHubTests
{
    private Mock<HubCallerContext> _mockContext = null!;
    private NotificationHub _hub = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockContext = new Mock<HubCallerContext>();
        _hub = new NotificationHub
        {
            Context = _mockContext.Object
        };
    }

    [TestMethod]
    public async Task OnConnectedAsync_SetsUserIdInItems_WhenUserAuthenticated()
    {
        var userId = "user123";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        var items = new Dictionary<object, object?>();

        _mockContext.Setup(c => c.User).Returns(principal);
        _mockContext.Setup(c => c.Items).Returns(items);

        await _hub.OnConnectedAsync();

        Assert.AreEqual(userId, items["UserId"]);
    }

    [TestMethod]
    public async Task OnConnectedAsync_DoesNotSetUserId_WhenUserNotAuthenticated()
    {
        var items = new Dictionary<object, object?>();
        _mockContext.Setup(c => c.User).Returns((ClaimsPrincipal?)null);
        _mockContext.Setup(c => c.Items).Returns(items);

        await _hub.OnConnectedAsync();

        Assert.IsFalse(items.ContainsKey("UserId"));
    }
}

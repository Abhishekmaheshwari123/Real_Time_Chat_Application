using System.Net;
using System.Security.Claims;
using ConnectHub.Chat.API.Controllers;
using ConnectHub.Chat.Application.Services;
using ConnectHub.Chat.Domain;
using ConnectHub.Chat.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace ConnectHub.Chat.API.Tests;

[TestClass]
public class ChatBusinessTests
{
    [TestMethod]
    public async Task SendMessage_PersistsMediaUrl_WhenProvided()
    {
        using var dbContext = CreateDbContext();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("http://local") };
        var accessor = new Mock<IHttpContextAccessor>();
        var chatService = new ChatService(httpClient, accessor.Object);
        var controller = new ChatController(dbContext, chatService);
        
        var claims = new[] { new Claim(ClaimTypes.Email, "alice@test.com") };
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) } };

        var dto = new SendMessageDto
        {
            Receiver = "bob@test.com",
            Content = "look at this",
            MediaUrl = "http://blob/img.png",
            MessageType = "image"
        };

        await controller.SendMessage(dto);

        var msg = await dbContext.Messages.FirstAsync();
        Assert.AreEqual("http://blob/img.png", msg.MediaUrl);
        Assert.AreEqual("image", msg.MessageType);
    }

    [TestMethod]
    public async Task ChatService_BuildsCorrectNotificationRequest()
    {
        var handler = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, c) => capturedRequest = r)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("http://notif-service") };
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer test-token";
        accessor.Setup(a => a.HttpContext).Returns(context);

        var service = new ChatService(httpClient, accessor.Object);

        await service.SendMessage("alice", "bob", "hello");

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(HttpMethod.Post, capturedRequest.Method);
        Assert.AreEqual("Bearer test-token", capturedRequest.Headers.Authorization?.ToString());
    }

    private static ChatDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseInMemoryDatabase($"chat-business-tests-{Guid.NewGuid()}")
            .Options;
        return new ChatDbContext(options);
    }
}

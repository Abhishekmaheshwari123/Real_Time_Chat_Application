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

namespace ConnectHub.Chat.API.Tests;

[TestClass]
public class ChatControllerTests
{
    [TestMethod]
    public async Task GetHistory_ReturnsUnauthorized_WhenClaimMissing()
    {
        using var dbContext = CreateDbContext();
        var controller = CreateController(dbContext, out _);

        var result = await controller.GetHistory("bob@connecthub.dev");

        Assert.IsInstanceOfType<UnauthorizedResult>(result);
    }

    [TestMethod]
    public async Task GetHistory_ReturnsOrderedMessages_ForConversation()
    {
        using var dbContext = CreateDbContext();
        dbContext.Messages.AddRange(
            new Message
            {
                Sender = "alice@connecthub.dev",
                Receiver = "bob@connecthub.dev",
                Content = "late",
                SentAt = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc)
            },
            new Message
            {
                Sender = "bob@connecthub.dev",
                Receiver = "alice@connecthub.dev",
                Content = "early",
                SentAt = new DateTime(2026, 1, 2, 9, 0, 0, DateTimeKind.Utc)
            },
            new Message
            {
                Sender = "other@connecthub.dev",
                Receiver = "alice@connecthub.dev",
                Content = "ignore",
                SentAt = new DateTime(2026, 1, 2, 8, 0, 0, DateTimeKind.Utc)
            });
        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext, out _);
        SetUser(controller, "alice@connecthub.dev");

        var result = await controller.GetHistory("bob@connecthub.dev");

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var messages = ((IEnumerable<Message>)okResult.Value!).ToList();
        Assert.AreEqual(2, messages.Count);
        Assert.AreEqual("early", messages[0].Content);
        Assert.AreEqual("late", messages[1].Content);
    }

    [TestMethod]
    public async Task GetUnreadCounts_ReturnsUnauthorized_WhenClaimMissing()
    {
        using var dbContext = CreateDbContext();
        var controller = CreateController(dbContext, out _);

        var result = await controller.GetUnreadCounts();

        Assert.IsInstanceOfType<UnauthorizedResult>(result);
    }

    [TestMethod]
    public async Task GetUnreadCounts_ReturnsGroupedUnreadCounts()
    {
        using var dbContext = CreateDbContext();
        dbContext.Messages.AddRange(
            new Message { Sender = "bob@connecthub.dev", Receiver = "alice@connecthub.dev", Content = "1", IsRead = false, SentAt = DateTime.UtcNow },
            new Message { Sender = "bob@connecthub.dev", Receiver = "alice@connecthub.dev", Content = "2", IsRead = false, SentAt = DateTime.UtcNow },
            new Message { Sender = "charlie@connecthub.dev", Receiver = "alice@connecthub.dev", Content = "3", IsRead = false, SentAt = DateTime.UtcNow },
            new Message { Sender = "charlie@connecthub.dev", Receiver = "alice@connecthub.dev", Content = "4", IsRead = true, SentAt = DateTime.UtcNow });
        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext, out _);
        SetUser(controller, "alice@connecthub.dev");

        var result = await controller.GetUnreadCounts();

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var counts = ((IEnumerable<object>)okResult.Value!)
            .ToDictionary(item => ReadProperty<string>(item, "user"), item => ReadProperty<int>(item, "count"));

        Assert.AreEqual(2, counts["bob@connecthub.dev"]);
        Assert.AreEqual(1, counts["charlie@connecthub.dev"]);
    }

    [TestMethod]
    public async Task GetConversations_ReturnsUnauthorized_WhenClaimMissing()
    {
        using var dbContext = CreateDbContext();
        var controller = CreateController(dbContext, out _);

        var result = await controller.GetConversations();

        Assert.IsInstanceOfType<UnauthorizedResult>(result);
    }

    [TestMethod]
    public async Task GetConversations_ReturnsSortedConversationSummaries()
    {
        using var dbContext = CreateDbContext();
        dbContext.Messages.AddRange(
            new Message
            {
                Sender = "alice@connecthub.dev",
                Receiver = "bob@connecthub.dev",
                Content = "old",
                SentAt = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                IsRead = true
            },
            new Message
            {
                Sender = "bob@connecthub.dev",
                Receiver = "alice@connecthub.dev",
                Content = "new",
                SentAt = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc),
                IsRead = false
            },
            new Message
            {
                Sender = "alice@connecthub.dev",
                Receiver = "charlie@connecthub.dev",
                Content = string.Empty,
                MessageType = "image",
                SentAt = new DateTime(2026, 1, 2, 11, 0, 0, DateTimeKind.Utc),
                IsRead = true
            });
        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext, out _);
        SetUser(controller, "alice@connecthub.dev");

        var result = await controller.GetConversations();

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var conversations = ((IEnumerable<object>)okResult.Value!).ToList();
        Assert.AreEqual(2, conversations.Count);

        Assert.AreEqual("bob@connecthub.dev", ReadProperty<string>(conversations[0], "User"));
        Assert.AreEqual("new", ReadProperty<string>(conversations[0], "LastMessage"));
        Assert.AreEqual(1, ReadProperty<int>(conversations[0], "UnreadCount"));

        Assert.AreEqual("charlie@connecthub.dev", ReadProperty<string>(conversations[1], "User"));
        Assert.AreEqual("[image]", ReadProperty<string>(conversations[1], "LastMessage"));
    }

    [TestMethod]
    public async Task SendMessage_ReturnsUnauthorized_WhenClaimMissing()
    {
        using var dbContext = CreateDbContext();
        var controller = CreateController(dbContext, out _);

        var result = await controller.SendMessage(new SendMessageDto
        {
            Receiver = "bob@connecthub.dev",
            Content = "hello"
        });

        Assert.IsInstanceOfType<UnauthorizedResult>(result);
    }

    [TestMethod]
    public async Task SendMessage_SavesMessage_NormalizesType_AndSendsNotification()
    {
        using var dbContext = CreateDbContext();
        var controller = CreateController(dbContext, out var recordingHandler);
        SetUser(controller, "alice@connecthub.dev");

        var result = await controller.SendMessage(new SendMessageDto
        {
            Receiver = "bob@connecthub.dev",
            Content = "hello",
            MessageType = " IMAGE "
        });

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var message = okResult.Value as Message;
        Assert.IsNotNull(message);
        Assert.AreEqual("image", message.MessageType);

        var savedMessage = await dbContext.Messages.SingleAsync();
        Assert.AreEqual("alice@connecthub.dev", savedMessage.Sender);
        Assert.AreEqual("bob@connecthub.dev", savedMessage.Receiver);
        Assert.AreEqual("image", savedMessage.MessageType);

        Assert.AreEqual(1, recordingHandler.CallCount);
        Assert.IsNotNull(recordingHandler.LastRequest);
        Assert.IsTrue(recordingHandler.LastRequest.RequestUri!.ToString().EndsWith("/notifications", StringComparison.OrdinalIgnoreCase));
    }

    private static ChatController CreateController(ChatDbContext dbContext, out RecordingHttpMessageHandler recordingHandler)
    {
        recordingHandler = new RecordingHttpMessageHandler();
        var httpClient = new HttpClient(recordingHandler)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };

        var notificationService = new ChatService(httpClient, httpContextAccessor);
        return new ChatController(dbContext, notificationService);
    }

    private static ChatDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseInMemoryDatabase($"chat-tests-{Guid.NewGuid()}")
            .Options;

        return new ChatDbContext(options);
    }

    private static void SetUser(ControllerBase controller, string email)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.NameIdentifier, email)
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };
    }

    private static T ReadProperty<T>(object value, string propertyName)
    {
        return (T)value.GetType().GetProperty(propertyName)!.GetValue(value)!;
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok")
            });
        }
    }
}
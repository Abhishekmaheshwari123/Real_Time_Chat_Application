using Microsoft.AspNetCore.SignalR;
using ConnectHub.Chat.Infrastructure;
using ConnectHub.Chat.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ConnectHub.Chat.API.Hubs;

[Authorize] // ✅ ENABLED
public class ChatHub : Hub
{
    private readonly ChatDbContext _context;

    private static readonly Dictionary<string, string> UserConnections = new();
    private static readonly object _lock = new();

    public ChatHub(ChatDbContext context)
    {
        _context = context;
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        lock (_lock)
        {
            var user = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

            if (user != null)
                UserConnections.Remove(user);
        }

        return base.OnDisconnectedAsync(exception);
    }

    public Task RegisterUser(string username)
    {
        lock (_lock)
        {
            UserConnections[username] = Context.ConnectionId;
        }

        return Task.CompletedTask;
    }

    public async Task SendMessage(string sender, string receiver, string message)
    {
        var chat = new Message
        {
            Sender = sender,
            Receiver = receiver,
            Content = message,
            SentAt = DateTime.UtcNow,
            Status = "Sent"
        };

        _context.Messages.Add(chat);
        await _context.SaveChangesAsync();

        string? receiverConn = null;

        lock (_lock)
        {
            UserConnections.TryGetValue(receiver, out receiverConn);
        }

        if (receiverConn != null)
        {
            chat.Status = "Delivered";
            await _context.SaveChangesAsync();

            await Clients.Client(receiverConn)
                .SendAsync("ReceiveMessage", sender, message, "Delivered");
        }

        await Clients.Caller
            .SendAsync("ReceiveMessage", sender, message, chat.Status);
    }

    public async Task Ping()
    {
        await Clients.Caller.SendAsync("Pong", "OK");
    }
}
using Microsoft.AspNetCore.SignalR;
using ConnectHub.Chat.Infrastructure;
using ConnectHub.Chat.Domain;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ConnectHub.Chat.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ChatDbContext _context;

    public ChatHub(ChatDbContext context)
    {
        _context = context;
    }

    // ================= SEND MESSAGE =================
    public async Task SendMessage(string receiver, string message)
    {
        var sender = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

        var chat = new Message
        {
            Sender = sender,
            Receiver = receiver,
            Content = message,
            SentAt = DateTime.UtcNow,
            Status = "Sent",
            IsRead = false
        };

        _context.Messages.Add(chat);
        await _context.SaveChangesAsync();

        var payload = new { 
            id = chat.Id, 
            sender = sender, 
            receiver = receiver, 
            message = message, 
            status = "Sent",
            sentAt = chat.SentAt
        };

        // ✅ 1. Send message to sender (✓ Sent)
        await Clients.Caller.SendAsync("ReceiveMessage", payload);

        // ✅ 2. Send message to receiver
        await Clients.User(receiver).SendAsync("ReceiveMessage", payload);

        // ✅ 3. Deliver ONLY if receiver is connected (basic check)
        // NOTE: this is simple approach (not perfect but works for your setup)
        try
        {
            // small delay to allow UI to render ✓ first
            await Task.Delay(200);

            // Re-fetch message to check current status
            var currentMsg = await _context.Messages.FindAsync(chat.Id);
            if (currentMsg != null && currentMsg.Status == "Sent")
            {
                currentMsg.Status = "Delivered";
                await _context.SaveChangesAsync();
                // notify sender → update to ✓✓
                await Clients.Caller.SendAsync("MessageDelivered", new { id = chat.Id });
            }
        }
        catch
        {
            // ignore if receiver not connected
        }

        // ✅ 4. Notification (unchanged)
        await Clients.User(receiver).SendAsync("ReceiveNotification", new { from = sender, message });
    }

    public async Task MarkAsSeen(string otherUser)
    {
        var currentUser = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        Console.WriteLine($"MarkAsSeen triggered by {currentUser} for messages from {otherUser}");
        
        // Find all unread messages from that specific user sent to me
        var messages = await _context.Messages
            .Where(m => m.Sender == otherUser && m.Receiver == currentUser && !m.IsRead)
            .ToListAsync();

        Console.WriteLine($"Found {messages.Count} unread messages to mark as seen.");
        
        if (!messages.Any()) return;

        foreach (var m in messages)
        {
            m.Status = "Seen";
            m.IsRead = true; // Mark as read so 'unread' endpoint returns 0
        }

        await _context.SaveChangesAsync();
        var ids = messages.Select(x => x.Id).ToList();

        // Sync both clients
        await Clients.User(otherUser).SendAsync("MessagesSeen", ids);
        await Clients.User(currentUser).SendAsync("MessagesSeen", ids);
    }   
}
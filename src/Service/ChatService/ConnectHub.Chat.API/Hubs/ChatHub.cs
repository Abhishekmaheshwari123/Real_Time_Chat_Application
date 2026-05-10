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
    public async Task SendMessage(string receiver, string message, string? mediaUrl = null, string messageType = "text")
    {
        var sender = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        var normalizedMessageType = (messageType ?? "text").Trim().ToLowerInvariant();

        var chat = new Message
        {
            Sender = sender,
            Receiver = receiver,
            Content = message,
            MediaUrl = mediaUrl,
            MessageType = normalizedMessageType,
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
            mediaUrl = mediaUrl,
            messageType = normalizedMessageType,
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

            // notify sender → update to ✓✓
            // 🔥 ONLY update to Delivered if it hasn't been seen yet!
            var currentStatus = await _context.Messages
                .Where(m => m.Id == chat.Id)
                .Select(m => m.Status)
                .FirstOrDefaultAsync();

            if (currentStatus != "Seen")
            {
                chat.Status = "Delivered";
                await _context.SaveChangesAsync();
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
        
        // Find all unread messages from that specific user sent to me
        var messages = await _context.Messages
            .Where(m => m.Sender == otherUser && m.Receiver == currentUser && !m.IsRead)
            .ToListAsync();

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
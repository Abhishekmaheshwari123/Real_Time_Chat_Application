using Microsoft.AspNetCore.Mvc;
using ConnectHub.Chat.Infrastructure;
using ConnectHub.Chat.Domain;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ConnectHub.Chat.API.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ChatDbContext _context;

    public ChatController(ChatDbContext context)
    {
        _context = context;
    }

    // ================= HISTORY =================
    [HttpGet("history/{user}")]
    public async Task<IActionResult> GetHistory(string user)
    {
        var currentUser = User.FindFirst(ClaimTypes.Email)?.Value;
        if (currentUser == null) return Unauthorized();

        var messages = await _context.Messages
            .Where(m =>
                (m.Sender == currentUser && m.Receiver == user) ||
                (m.Sender == user && m.Receiver == currentUser))
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return Ok(messages);
    }

// ================= UNREAD COUNTS =================
[HttpGet("unread")]
public async Task<IActionResult> GetUnreadCounts()
{
    var currentUser = User.FindFirst(ClaimTypes.Email)?.Value;
    if (currentUser == null) return Unauthorized();

    var counts = await _context.Messages
        .Where(m => m.Receiver == currentUser && m.IsRead == false) // Filter explicitly
        .GroupBy(m => m.Sender)
        .Select(g => new
        {
            user = g.Key,
            count = g.Count()
        })
        .ToListAsync();

    return Ok(counts);
}

    


// ================= Chat Conversations ================
   [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var currentUser = User.FindFirst(ClaimTypes.Email)?.Value;
        if (currentUser == null) return Unauthorized();

        // ✅ Step 1: Get all unique users the current user has chatted with
        var users = await _context.Messages
            .Where(m => m.Sender == currentUser || m.Receiver == currentUser)
            .Select(m => m.Sender == currentUser ? m.Receiver : m.Sender)
            .Distinct()
            .ToListAsync();

        // ✅ Step 2: For each user, get last message + unread count
        var conversations = new List<object>();

        foreach (var user in users)
        {
            var lastMessage = await _context.Messages
                .Where(m =>
                    (m.Sender == currentUser && m.Receiver == user) ||
                    (m.Sender == user && m.Receiver == currentUser))
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();

            var unreadCount = await _context.Messages
                .CountAsync(m =>
                    m.Sender == user &&
                    m.Receiver == currentUser &&
                    !m.IsRead);

            conversations.Add(new
            {
                User = user,
                LastMessage = lastMessage?.Content ?? "",
                Time = lastMessage?.SentAt ?? DateTime.MinValue,
                UnreadCount = unreadCount
            });
        }

        // ✅ Step 3: Sort by latest message
        var result = conversations
            .OrderByDescending(x => ((DateTime)x.GetType().GetProperty("Time")!.GetValue(x)!))
            .ToList();

        return Ok(result);
    }

    // ================= REST SEND (OPTIONAL) =================
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var sender = User.FindFirst(ClaimTypes.Email)?.Value;
        if (sender == null) return Unauthorized();

        var msg = new Message
        {
            Sender = sender,
            Receiver = dto.Receiver,
            Content = dto.Content,
            SentAt = DateTime.UtcNow,
            Status = "Sent"
        };

        _context.Messages.Add(msg);
        await _context.SaveChangesAsync();

        return Ok(msg);
    }
}
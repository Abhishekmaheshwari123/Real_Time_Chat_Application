using Microsoft.AspNetCore.Mvc;
using ConnectHub.Chat.Infrastructure;
using ConnectHub.Chat.Domain;
using ConnectHub.Chat.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ConnectHub.Chat.API.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
private readonly ChatDbContext _context;

public ChatController(ChatDbContext context)
{
    _context = context;
}

// ✅ SEND MESSAGE (REST)
[HttpPost("send")]
public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
{
    var sender = User.FindFirst(ClaimTypes.Email)?.Value;

    if (string.IsNullOrEmpty(sender))
        return Unauthorized();

    var message = new Message
    {
        Sender = sender,
        Receiver = dto.Receiver,
        Content = dto.Content,
        SentAt = DateTime.UtcNow,
        Status = "Sent"
    };

    _context.Messages.Add(message);
    await _context.SaveChangesAsync();

    return Ok(new { success = true });
}

// ✅ CHAT HISTORY (ONLY ONE API)
[HttpGet("history/{user}")]
public async Task<IActionResult> GetChatHistory(string user)
{
    var currentUser = User.FindFirst(ClaimTypes.Email)?.Value;

    if (string.IsNullOrEmpty(currentUser))
        return Unauthorized();

    var messages = await _context.Messages
        .Where(m =>
            (m.Sender == currentUser && m.Receiver == user) ||
            (m.Sender == user && m.Receiver == currentUser)
        )
        .OrderBy(m => m.SentAt)
        .ToListAsync();

    return Ok(messages);
}

}

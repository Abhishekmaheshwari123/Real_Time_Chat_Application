using Microsoft.AspNetCore.Mvc;
using ConnectHub.Chat.Infrastructure;
using ConnectHub.Chat.Domain;
using ConnectHub.Chat.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ConnectHub.Chat.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

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

    // ==============================
    // ✅ 1. SEND MESSAGE (REST API)
    // ==============================
    [HttpPost("send")]
    [Authorize]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        //GET USER FROM TOKEN
        var sender = User.Identity?.Name;

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

    // ======================================
    //2. GET CHAT HISTORY BETWEEN USERS
    // ======================================
    [HttpGet("history")]
    public async Task<IActionResult> GetChatHistory(string user1, string user2)
    {
        var messages = await _context.Messages
            .Where(m =>
                (m.Sender == user1 && m.Receiver == user2) ||
                (m.Sender == user2 && m.Receiver == user1)
            )
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto
            {
                Sender = m.Sender,
                Receiver = m.Receiver,
                Content = m.Content,
                Timestamp = m.SentAt
            })
            .ToListAsync();

        return Ok(messages);
    }
}
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ConnectHub.Chat.API.Hubs;

public class EmailUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(ClaimTypes.Email)?.Value;
    }
}
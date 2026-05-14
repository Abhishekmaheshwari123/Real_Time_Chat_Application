using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace ConnectHub.Chat.Application.Services
{
    public class ChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatService(HttpClient httpClient, IHttpContextAccessor accessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = accessor;
        }

        public async Task SendMessage(string senderId, string receiverId, string messageText)
        {
            var messageId = Guid.NewGuid();

            var httpContext = _httpContextAccessor.HttpContext;

            var token = httpContext?.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(token))
            {
                var accessToken = httpContext?.Request.Query["access_token"].ToString();
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    token = $"Bearer {accessToken}";
                }
            }

            var notification = new
            {
                RecipientId = receiverId,
                Type = "MESSAGE",
                RelatedId = messageId,
                Message = $"New message from {senderId}"
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/notifications"
            );

            request.Content = JsonContent.Create(notification);

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.TryAddWithoutValidation("Authorization", token);
            }

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ Notification service failed");
            }
        }
    }
}
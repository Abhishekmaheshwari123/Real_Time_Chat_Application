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

            // 🔥 GET JWT TOKEN FROM CURRENT REQUEST
            var token = _httpContextAccessor.HttpContext?
                .Request.Headers["Authorization"]
                .ToString();

            var notification = new
            {
                RecipientId = receiverId,
                Type = "MESSAGE",
                RelatedId = messageId,
                Message = $"New message from {senderId}"
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "http://localhost:5226/notifications"
            );

            request.Content = JsonContent.Create(notification);

            // 🔥 FORWARD TOKEN
            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ Notification service failed");
            }
        }
    }
}
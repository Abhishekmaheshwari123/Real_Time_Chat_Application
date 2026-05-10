namespace ConnectHub.Chat.API.DTOs;

public class SendMessageRequest
{
    public string Sender { get; set; }
    public string Receiver { get; set; }
    public string Content { get; set; }
    public string? MediaUrl { get; set; }
    public string MessageType { get; set; } = "text";
}
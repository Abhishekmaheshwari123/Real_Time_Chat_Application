public class SendMessageDto
{
    public string Receiver { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public string MessageType { get; set; } = "text";
}
namespace ConnectHub.Chat.Domain;

public class Message
{
    public int Id { get; set; }

    public string Sender { get; set; }
    public string Receiver { get; set; }

    public string Content { get; set; }
    public string? MediaUrl { get; set; }
    public string MessageType { get; set; } = "text"; // text, image, video, file

    public DateTime SentAt { get; set; }

    public string Status { get; set; } = "Sent";

    public bool IsRead { get; set; } = false;
}
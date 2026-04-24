public class Message
{
    public int Id { get; set; }

    public string Sender { get; set; }
    public string Receiver { get; set; }

    public string Content { get; set; }

    public DateTime SentAt { get; set; }

    public string Status { get; set; } = "Sent"; // ✅ NEW
}
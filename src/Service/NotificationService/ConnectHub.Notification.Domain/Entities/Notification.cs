namespace ConnectHub.Notification.Domain.Entities
{
    public class Notification
{
    public Guid Id { get; set; }

    public string RecipientId { get; set; } = default!;

    // 🔥 NEW
    public string Type { get; set; } = default!; 
    // MESSAGE / MENTION / ROOM_INVITE / ROLE_CHANGE

    // 🔥 NEW
    public Guid? RelatedId { get; set; }

    public string Message { get; set; } = default!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}
}
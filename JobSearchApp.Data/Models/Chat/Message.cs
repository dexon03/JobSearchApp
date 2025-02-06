namespace JobSearchApp.Data.Models.Chat;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime TimeStamp { get; set; }
    public bool IsRead { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public int ChatId { get; set; }
    public User Receiver { get; set; } = null!;
    public User Sender { get; set; } = null!;
    public Chat Chat { get; set; } = null!;
}
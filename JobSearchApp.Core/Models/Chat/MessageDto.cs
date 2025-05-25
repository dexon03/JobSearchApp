using JobSearchApp.Data.Models;

namespace JobSearchApp.Core.Models.Chat;

public record MessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime TimeStamp { get; set; }
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
    public bool IsSender { get; set; }
    public int ChatId { get; set; }
    public bool IsRead { get; set; }
};
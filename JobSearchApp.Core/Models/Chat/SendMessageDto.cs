namespace JobSearchApp.Core.Models.Chat;

public record SendMessageDto
{
    public required string Content { get; set; }
    public required string SenderName { get; set; }
    public int ReceiverId { get; set; }
    public int ChatId { get; set; }
};
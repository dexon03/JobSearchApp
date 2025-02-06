namespace JobSearchApp.Core.Models.Chat;

public record SendMessageDto
{
    public string Content { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; }
    public int ReceiverId { get; set; }
    public int ChatId { get; set; }
};
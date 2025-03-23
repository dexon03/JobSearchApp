namespace JobSearchApp.Core.Models.Chat;

public record CreateChatDto
{
    public int? SenderId { get; set; }
    public string SenderName { get; set; }
    public int ReceiverId { get; set; }
    public string ReceiverName { get; set; }
    public string Message { get; set; }
}
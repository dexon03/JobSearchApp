namespace JobSearchApp.Core.Models.Chat;

public record ChatDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? SenderOfLastMessage { get; set; }
    public string LastMessage { get; set; }
    public bool IsLastMessageRead { get; set; }
}
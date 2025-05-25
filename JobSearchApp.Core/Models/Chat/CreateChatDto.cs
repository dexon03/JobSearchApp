using JobSearchApp.Data.Models;

namespace JobSearchApp.Core.Models.Chat;

public record CreateChatDto
{
    public User Sender { get; set; } = null!;
    public int ReceiverId { get; set; }
    public string ReceiverName { get; init; } = null!;
    public int VacancyId { get; set; }
    public required string Message { get; set; }
}
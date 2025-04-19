using JobSearchApp.Data.Models;

namespace JobSearchApp.Core.Models.Chat;

public record CreateChatDto
{
    public User Sender { get; set; }
    public int ReceiverId { get; set; }
    public string ReceiverName { get; set; }
    public int VacancyId { get; set; }
    public string Message { get; set; }
}
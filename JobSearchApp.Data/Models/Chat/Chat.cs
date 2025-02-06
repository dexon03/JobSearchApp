namespace JobSearchApp.Data.Models.Chat;

public class Chat
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = [];
}
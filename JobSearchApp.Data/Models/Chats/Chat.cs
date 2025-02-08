namespace JobSearchApp.Data.Models.Chats;

public class Chat
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = [];
}
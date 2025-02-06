using JobSearchApp.Core.Models.Chat;

namespace JobSearchApp.Core.Contracts.Chats;

public interface IChatService
{
    Task<List<ChatDto>> GetChatList(int userId, int pageNumber, int pageSize);
    Task<List<MessageDto>> GetChatMessages(int chatId);
    Task CreateChat(CreateChatDto chatDto);
}
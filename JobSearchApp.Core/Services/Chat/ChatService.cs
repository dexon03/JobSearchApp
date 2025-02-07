using JobSearchApp.Core.Contracts.Chats;
using JobSearchApp.Core.Models.Chat;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Chat;
using Microsoft.EntityFrameworkCore;

namespace JobSearchApp.Core.Services.Chat;

public class ChatService(AppDbContext db) : IChatService
{
    public async Task<List<ChatDto>> GetChatList(int userId, int pageNumber, int pageSize)
    {
        var chats = await db.Messages
            .Include(m => m.Chat)
            .AsSplitQuery()
            .Where(m => m.ReceiverId == userId || m.SenderId == userId)
            .OrderByDescending(x => x.TimeStamp)
            .GroupBy(m => m.Chat)
            .Select(g => new ChatDto
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                SenderOfLastMessage = g.OrderByDescending(m => m.TimeStamp).First().Sender.UserName!,
                LastMessage = g.OrderByDescending(m => m.TimeStamp).First().Content,
                IsLastMessageRead = g.OrderByDescending(m => m.TimeStamp).First().IsRead
            })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return chats;
    }

    public async Task<List<MessageDto>> GetChatMessages(int chatId)
    {
        var messages = await db.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .AsSplitQuery()
            .Where(m => m.ChatId == chatId)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                ChatId = m.ChatId,
                Content = m.Content,
                TimeStamp = m.TimeStamp,
                Sender = m.Sender,
                Receiver = m.Receiver,
                IsRead = m.IsRead
            })
            .OrderBy(m => m.TimeStamp)
            .ToListAsync();
        return messages;
    }

    public async Task CreateChat(CreateChatDto chatDto)
    {
        var existentChat = await db.Messages
            .FirstOrDefaultAsync(m => (m.ReceiverId == chatDto.ReceiverId && m.SenderId == chatDto.SenderId)
                                      || (m.ReceiverId == chatDto.SenderId && m.SenderId == chatDto.ReceiverId));
        if (existentChat is not null)
        {
            var newMessage = new Message
            {
                ChatId = existentChat.ChatId,
                Content = chatDto.Message,
                TimeStamp = DateTime.Now,
                SenderId = chatDto.SenderId,
                ReceiverId = chatDto.ReceiverId,
                IsRead = false
            };
            db.Messages.Add(newMessage);
            await db.SaveChangesAsync();
        }
        else
        {
            await CreateNewChatAndAddMessage(chatDto);
        }
    }

    private async Task CreateNewChatAndAddMessage(CreateChatDto chatDto)
    {
        var chat = new Data.Models.Chat.Chat
        {
            Name = chatDto.SenderName + " / " + chatDto.ReceiverName,
        };

        var message = new Message
        {
            Chat = chat,
            Content = chatDto.Message,
            TimeStamp = DateTime.Now,
            SenderId = chatDto.SenderId,
            ReceiverId = chatDto.ReceiverId,
            IsRead = false
        };

        db.Chats.Add(chat);
        db.Messages.Add(message);
        await db.SaveChangesAsync();
    }
}
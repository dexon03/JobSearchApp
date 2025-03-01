using JobSearchApp.Core.Contracts.Chats;
using JobSearchApp.Core.Models.Chat;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Chats;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.Core.Services.Chats;

public class ChatService(
    AppDbContext db,
    IFusionCache hybridCache,
    ILogger logger) : IChatService
{
    private readonly ILogger _logger = logger.ForContext<ChatService>();

    public ValueTask<List<ChatDto>> GetChatList(int userId, int pageNumber, int pageSize)
    {
        var cacheKey = $"chat_list_{userId}_{pageNumber}_{pageSize}";
        var cacheTag = $"user_chats_{userId}";

        return hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _logger.Information("Cache miss for {CacheKey}. Fetching from database...", cacheKey);

                // var lastMessages = db.Messages
                //     .Where(m => m.ReceiverId == userId || m.SenderId == userId)
                //     .GroupBy(m => m.ChatId)
                //     .Select(g => new
                //     {
                //         ChatId = g.Key,
                //         LastMessage = g.OrderByDescending(m => m.TimeStamp).First()
                //     });

                var chats = await db.Chats
                    .Where(c => db.Messages
                        .Any(m => (m.ReceiverId == userId || m.SenderId == userId) && m.ChatId == c.Id))
                    .OrderByDescending(c => c.Messages.OrderByDescending(m => m.TimeStamp).FirstOrDefault()!.TimeStamp)
                    .Select(c => new ChatDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        SenderOfLastMessage = c.Messages.OrderByDescending(m => m.TimeStamp).FirstOrDefault()!.Sender.UserName,
                        LastMessage = c.Messages.OrderByDescending(m => m.TimeStamp).FirstOrDefault()!.Content,
                        IsLastMessageRead = c.Messages.OrderByDescending(m => m.TimeStamp).FirstOrDefault()!.IsRead
                    })
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken: ctx);


                _logger.Information("Fetched {Count} chats from database for user {UserId}.", chats.Count, userId);
                return chats;
            },
            TimeSpan.FromMinutes(1),
            [cacheTag]
        );
    }

    public async Task<List<MessageDto>> GetChatMessages(int chatId)
    {
        var cacheKey = $"chat_messages_{chatId}";
        var cacheTag = $"chat_{chatId}";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                _logger.Information("Cache miss for {CacheKey}. Fetching messages from database...", cacheKey);

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

                _logger.Information("Fetched {Count} messages for chat {ChatId}.", messages.Count, chatId);
                return messages;
            },
            TimeSpan.FromMinutes(2),
            [cacheTag]
        );
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
                TimeStamp = DateTime.UtcNow,
                SenderId = chatDto.SenderId,
                ReceiverId = chatDto.ReceiverId,
                IsRead = false
            };
            db.Messages.Add(newMessage);
            await db.SaveChangesAsync();

            _logger.Information("New message added to chat {ChatId} by user {SenderId}.", existentChat.ChatId,
                chatDto.SenderId);

            await InvalidateChatCacheAsync(existentChat.ChatId, chatDto.SenderId, chatDto.ReceiverId);
        }
        else
        {
            await CreateNewChatAndAddMessage(chatDto);
        }
    }

    private async Task CreateNewChatAndAddMessage(CreateChatDto chatDto)
    {
        var chat = new Chat
        {
            Name = $"{chatDto.SenderName} / {chatDto.ReceiverName}",
        };

        var message = new Message
        {
            Chat = chat,
            Content = chatDto.Message,
            TimeStamp = DateTime.UtcNow,
            SenderId = chatDto.SenderId,
            ReceiverId = chatDto.ReceiverId,
            IsRead = false
        };

        db.Chats.Add(chat);
        db.Messages.Add(message);
        await db.SaveChangesAsync();

        _logger.Information("Created new chat between {SenderId} and {ReceiverId}.", chatDto.SenderId,
            chatDto.ReceiverId);

        await InvalidateChatCacheAsync(null, chatDto.SenderId, chatDto.ReceiverId);
    }

    private async Task InvalidateChatCacheAsync(int? chatId, int senderId, int receiverId)
    {
        var tasks = new List<Task>();

        if (chatId.HasValue)
        {
            tasks.Add(hybridCache.RemoveByTagAsync($"chat_{chatId.Value}").AsTask());
        }

        tasks.Add(hybridCache.RemoveByTagAsync($"user_chats_{senderId}").AsTask());
        tasks.Add(hybridCache.RemoveByTagAsync($"user_chats_{receiverId}").AsTask());

        try
        {
            await Task.WhenAll(tasks);
            _logger.Information("Cache invalidated for chat {ChatId}, sender {SenderId}, receiver {ReceiverId}.",
                chatId,
                senderId, receiverId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Cache invalidation failed for chat {ChatId}, sender {SenderId}, receiver {ReceiverId}.",
                chatId, senderId, receiverId);
        }
    }
}
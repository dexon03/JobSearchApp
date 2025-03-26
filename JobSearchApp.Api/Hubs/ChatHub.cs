using System.Security.Claims;
using JobSearchApp.Core.Models.Chat;
using JobSearchApp.Data;
using JobSearchApp.Data.Models;
using JobSearchApp.Data.Models.Chats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace JobSearchApp.Api.Hubs;

[Authorize]
public class ChatHub(AppDbContext db) : Hub
{
    public Task JoinChatGroup(string chatId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, chatId);
    }

    public Task LeaveChatGroup(string chatId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
    }

    public async Task SendMessage(SendMessageDto messageDto)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            throw new HubException("User is not authenticated.");
        }

        var messageEntity = new Message
        {
            Content = messageDto.Content,
            SenderId = int.Parse(userId),
            ReceiverId = messageDto.ReceiverId,
            ChatId = messageDto.ChatId,
            TimeStamp = DateTime.UtcNow,
        };
        db.Messages.Add(messageEntity);
        await db.SaveChangesAsync();

        var message = new MessageDto
        {
            Id = messageEntity.Id,
            Content = messageDto.Content,
            Sender = new User
            {
                Id = int.Parse(userId),
                UserName = messageDto.SenderName
            },

            ChatId = messageDto.ChatId,
            TimeStamp = DateTime.Now,
        };

        await Clients.Group(messageDto.ChatId.ToString()).SendAsync("ReceiveMessage", message);
    }

    public override Task OnConnectedAsync()
    {
        if (Context.User is { Identity: not null })
            return Clients.All.SendAsync("ConnectedUser", $"{Context.User.Identity.Name} joined the chat");
        return Task.CompletedTask;
    }
}
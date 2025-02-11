using JobSearchApp.Core.Contracts.Chats;
using JobSearchApp.Core.Models.Chat;

namespace JobSearchApp.Api.Endpoints;

public static class ChatEndpoints
{
    public static void Register(RouteGroupBuilder group)
    {
        var chatGroup = group.MapGroup("/chat")
            .RequireAuthorization();

        chatGroup.MapGet("/list", async (
                int userId,
                int pageNumber,
                int pageSize,
                IChatService chatService) =>
            {
                var chats = await chatService.GetChatList(userId, pageNumber, pageSize);
                return Results.Ok(chats);
            })
            .WithName("GetChatList")
            .WithOpenApi();

        chatGroup.MapGet("/messages/{chatId}", async (
                int chatId,
                IChatService chatService) =>
            {
                var messages = await chatService.GetChatMessages(chatId);
                return Results.Ok(messages);
            })
            .WithName("GetChatMessages")
            .WithOpenApi();

        chatGroup.MapPost("", async (
                CreateChatDto chatDto,
                IChatService chatService) =>
            {
                await chatService.CreateChat(chatDto);
                return Results.Ok();
            })
            .WithName("CreateChat")
            .WithOpenApi();
    }
}
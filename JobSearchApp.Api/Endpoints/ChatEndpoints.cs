using System.Security.Claims;
using JobSearchApp.Core.Contracts.Chats;
using JobSearchApp.Core.Models.Chat;
using JobSearchApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobSearchApp.Api.Endpoints;

public static class ChatEndpoints
{
    public static void Register(RouteGroupBuilder group)
    {
        var chatGroup = group.MapGroup("/chat")
            .RequireAuthorization();

        chatGroup.MapGet("list", async (
                [FromQuery] int page,
                [FromQuery] int pageSize,
                ClaimsPrincipal claims,
                UserManager<User> userManager,
                IChatService chatService) =>
            {
                var userId = int.Parse(userManager.GetUserId(claims)!);
                var chats = await chatService.GetChatList(userId, page, pageSize);
                return Results.Ok(chats);
            })
            .WithName("GetChatList")
            .WithOpenApi();

        chatGroup.MapGet("/messages/{chatId}", async (
                int chatId,
                ClaimsPrincipal claims,
                UserManager<User> userManager,
                IChatService chatService) =>
            {
                var userId = int.Parse(userManager.GetUserId(claims)!);
                var messages = await chatService.GetChatMessages(chatId, userId);
                return Results.Ok(messages);
            })
            .WithName("GetChatMessages")
            .WithOpenApi();

        chatGroup.MapPost("", async (
                CreateChatDto chatDto,
                ClaimsPrincipal claims,
                UserManager<User> userManager,
                IChatService chatService) =>
            {
                var user = await userManager.GetUserAsync(claims)!;
                chatDto.Sender = user!;
                await chatService.CreateChat(chatDto);
                return Results.Ok();
            })
            .WithName("CreateChat")
            .WithOpenApi();
    }
}
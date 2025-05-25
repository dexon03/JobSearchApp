using Helpers;
using JobSearchApp.Core.Models.Chat;
using JobSearchApp.Core.Services.Chats;
using JobSearchApp.Data;
using JobSearchApp.Data.Models;
using JobSearchApp.Data.Models.Chats;
using JobSearchApp.Data.Models.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Chats.ChatServiceTests;

public class CreateChatService
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly ChatService _service;

    public CreateChatService()
    {
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        _dbMock = new Mock<IAppDbContext>();
        _cacheMock = new Mock<IFusionCache>();
        var baseLoggerMock = new Mock<ILogger>();
        var contextualLoggerMock = new Mock<ILogger>();

        baseLoggerMock
            .Setup(l => l.ForContext<ChatService>())
            .Returns(contextualLoggerMock.Object);
        _loggerMock = baseLoggerMock;

        _service = new ChatService(_dbMock.Object, _userManagerMock.Object, _cacheMock.Object, baseLoggerMock.Object);
    }

    [Fact]
    public async Task CreateChat_Should_Add_Message_To_Existing_Chat()
    {
        // Arrange
        var sender = new User { Id = 1 };
        var existingMessage = new Message { ChatId = 42, SenderId = 1, ReceiverId = 2 };
        var messages = new List<Message> { existingMessage };

        var mockMessages = EfHelpers.CreateMockDbSet(messages);
        _dbMock.Setup(db => db.Messages).Returns(mockMessages.Object);

        _userManagerMock.Setup(u => u.IsInRoleAsync(sender, "Candidate"))
            .ReturnsAsync(true);

        var mockVacancyUsers = EfHelpers.CreateMockDbSet(new List<VacancyUser>());
        _dbMock.Setup(db => db.VacancyUsers).Returns(mockVacancyUsers.Object);

        var chatDto = new CreateChatDto
        {
            Sender = sender,
            ReceiverId = 2,
            ReceiverName = "Receiver",
            VacancyId = 99,
            Message = "Hello"
        };

        // Act
        await _service.CreateChat(chatDto);

        // Assert
        _dbMock.Verify(db => db.Messages.Add(It.Is<Message>(m => m.ChatId == 42 && m.Content == "Hello")), Times.Once);
        _dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _loggerMock.Verify(l => l.ForContext<ChatService>().Information(It.IsAny<string>(), 42, 1), Times.Once);
        _cacheMock.Verify(c => c.RemoveByTagAsync("chat_42", It.IsAny<FusionCacheEntryOptions?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateChat_Should_Create_New_Chat_If_Not_Exists()
    {
        // Arrange
        var sender = new User { Id = 1, FirstName = "John", LastName = "Smith" };
        var chatDto = new CreateChatDto
        {
            Sender = sender,
            ReceiverId = 2,
            ReceiverName = "Jane",
            VacancyId = 100,
            Message = "Hi"
        };

        var mockMessages = EfHelpers.CreateMockDbSet(new List<Message>());
        var mockChats = new Mock<DbSet<Chat>>();
        var mockMessagesSet = new Mock<DbSet<Message>>();
        var mockVacancyUsers = new Mock<DbSet<VacancyUser>>();

        _dbMock.Setup(db => db.Messages).Returns(mockMessages.Object);
        _dbMock.Setup(db => db.Chats).Returns(mockChats.Object);
        _dbMock.Setup(db => db.VacancyUsers).Returns(mockVacancyUsers.Object);
        _dbMock.Setup(db => db.Messages.Add(It.IsAny<Message>()));
        _dbMock.Setup(db => db.Chats.Add(It.IsAny<Chat>()));
        _dbMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _userManagerMock.Setup(u => u.IsInRoleAsync(sender, "Candidate")).ReturnsAsync(true);

        // Act
        await _service.CreateChat(chatDto);

        // Assert
        _dbMock.Verify(db => db.Chats.Add(It.IsAny<Chat>()), Times.Once);
        _dbMock.Verify(db => db.Messages.Add(It.IsAny<Message>()), Times.Once);
        _dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _loggerMock.Verify(l => l.ForContext<ChatService>().Information(It.Is<string>(msg => msg.Contains("Created new chat")), 1, 2),
            Times.Once);
    }
}
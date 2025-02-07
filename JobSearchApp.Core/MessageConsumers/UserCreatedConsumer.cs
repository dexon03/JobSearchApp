using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Core.MessageContracts;
using JobSearchApp.Core.Models.Profiles;
using JobSearchApp.Data.Enums;
using MassTransit;

namespace JobSearchApp.Core.MessageConsumers;

public sealed class UserCreatedConsumer(IProfileService profileService) : IConsumer<UserCreatedEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var message = context.Message;
        var profile = new ProfileCreateDto
        {
            UserId = message.UserId,
            Name = message.FirstName,
            Surname = message.LastName,
            Email = message.Email,
            PhoneNumber = message.PhoneNumber,
            Role = Enum.Parse<Role>(message.Role)
        };
        
        await profileService.CreateProfile(profile);
    }
}
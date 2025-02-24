using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Core.MessageContracts;
using MassTransit;

namespace JobSearchApp.Core.MessageConsumers;

public class UserDeletedEventConsumer(IProfileService profileService) : IConsumer<UserDeletedEvent>
{
    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var userId = context.Message.UserId;
        await profileService.DeleteProfileByUserId(userId);
    }
}
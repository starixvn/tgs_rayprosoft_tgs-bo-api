using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.SmsService.Client;

namespace TGS.Services.BackOffice.ApplicationServices.Sms;

internal class RemoveSmsSettingCommandService(ISmsServiceClient client) : ICommandService<RemoveSmsSettingCommand>
{
    public ValueTask ExecuteAsync(RemoveSmsSettingCommand command, CancellationToken cancellationToken = default)
        => client.RemoveSmsSettingAsync(command.SmsSettingId, cancellationToken);
}
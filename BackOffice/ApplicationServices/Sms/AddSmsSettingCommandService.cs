using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.SmsService.Client;

namespace TGS.Services.BackOffice.ApplicationServices.Sms;

internal class AddSmsSettingCommandService(ISmsServiceClient client) : ICommandService<AddSmsSettingCommand>
{
    public ValueTask ExecuteAsync(AddSmsSettingCommand command, CancellationToken cancellationToken = default)
        => client.AddSmsSettingAsync(command.Name, command.Code, command.Setting, command.Modifier, cancellationToken);
}
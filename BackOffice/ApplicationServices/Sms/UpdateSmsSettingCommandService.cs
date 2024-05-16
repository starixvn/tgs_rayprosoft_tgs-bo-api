using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.SmsService.Client;

namespace TGS.Services.BackOffice.ApplicationServices.Sms;

internal class UpdateSmsSettingCommandService(ISmsServiceClient client) : ICommandService<UpdateSmsSettingCommand>
{
    public ValueTask ExecuteAsync(UpdateSmsSettingCommand command, CancellationToken cancellationToken = default)
        => client.UpdateSmsSettingAsync(
            command.SmsSettingId, 
            command.Name, 
            command.Code, 
            command.Setting, 
            command.Modifier, 
            cancellationToken);
}
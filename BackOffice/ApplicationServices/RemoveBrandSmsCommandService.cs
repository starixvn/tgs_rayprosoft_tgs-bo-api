using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.SmsService.Client;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class RemoveBrandSmsCommandService(ISmsServiceClient client) : ICommandService<RemoveBrandSmsCommand>
{
    public ValueTask ExecuteAsync(RemoveBrandSmsCommand command, CancellationToken cancellationToken = default)
        => client.RemoveBrandSmsAsyncAsync(command.BrandId, command.SmsSettingId, cancellationToken);
}
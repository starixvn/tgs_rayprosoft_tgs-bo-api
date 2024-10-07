using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.SmsService.Client;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class ApplyBrandSmsCommandService(ISmsServiceClient client) : ICommandService<ApplyBrandSmsCommand>
{
    public ValueTask ExecuteAsync(ApplyBrandSmsCommand command, CancellationToken cancellationToken = default)
        => client.ApplyBrandSmsAsync(command.BrandId, command.SmsSettingId, command.Modifier, cancellationToken);
}
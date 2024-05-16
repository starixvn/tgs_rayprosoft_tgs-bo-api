using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.SmsService.Client;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class AddBrandSmsCommandService(ISmsServiceClient client) : ICommandService<AddBrandSmsCommand>
{
    public ValueTask ExecuteAsync(AddBrandSmsCommand command, CancellationToken cancellationToken = default)
        => client.AddBrandSmsAsync(command.BrandId, command.SmsSettingId, cancellationToken);
}
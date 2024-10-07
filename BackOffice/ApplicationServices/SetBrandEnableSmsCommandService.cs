using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class SetBrandEnableSmsCommandService(IBrandServiceClient client) : ICommandService<SetBrandEnableSmsCommand>
{
    public ValueTask ExecuteAsync(SetBrandEnableSmsCommand command, CancellationToken cancellationToken = default)
        => client.SetBrandEnableSmsAsync(command.BrandId, command.EnableSms, command.Modifier, cancellationToken);
}
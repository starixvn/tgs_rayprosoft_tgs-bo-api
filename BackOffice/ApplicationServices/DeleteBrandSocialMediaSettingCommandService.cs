using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class DeleteBrandSocialMediaSettingCommandService(IBrandServiceClient brandServiceClient)
    : ICommandService<DeleteBrandSocialMediaSettingCommand>
{
    public ValueTask ExecuteAsync(DeleteBrandSocialMediaSettingCommand command, CancellationToken cancellationToken = default)
        => brandServiceClient.DeleteSocialMediaSettingAsync(command.BrandId, command.SettingId, cancellationToken);
}

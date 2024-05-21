using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class SetBrandSocialMediaSettingOrderCommandService(IBrandServiceClient brandServiceClient)
    : ICommandService<SetBrandSocialMediaSettingOrderCommand>
{
    public ValueTask ExecuteAsync(SetBrandSocialMediaSettingOrderCommand command, CancellationToken cancellationToken = default)
        => brandServiceClient.SetSocialMediaSettingOrderAsync(
            command.SettingId,
            command.NewOrder,
            command.Modifier,
            cancellationToken);
}

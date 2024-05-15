using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class AddBrandSocialMediaSettingCommandService(IBrandServiceClient brandServiceClient)
    : IGetService<AddBrandSocialMediaSettingCommand, Guid>
{
    public ValueTask<Guid> GetAsync(AddBrandSocialMediaSettingCommand command, CancellationToken cancellationToken = default)
        => brandServiceClient.AddSocialMediaSettingAsync(
            command.BrandId,
            command.Setting,
            command.Modifier,
            cancellationToken);
}

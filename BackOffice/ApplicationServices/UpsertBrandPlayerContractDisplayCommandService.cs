using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class UpsertBrandPlayerContractDisplayCommandService(IBrandServiceClient brandServiceClient)
    : ICommandService<UpsertBrandPlayerContractDisplayCommand>
{
    private readonly PlayerPropertyName[] _contractType =
        [
            PlayerPropertyName.Line,
            PlayerPropertyName.Viber,
            PlayerPropertyName.Telegram,
            PlayerPropertyName.FacebookMessenger,
            PlayerPropertyName.WhatsApp,
            PlayerPropertyName.Qq,
            PlayerPropertyName.Wechat,
            PlayerPropertyName.Zalo
        ];

    public async ValueTask ExecuteAsync(UpsertBrandPlayerContractDisplayCommand command, CancellationToken cancellationToken = default)
    {
        if (command.UpdateTypes.Any() is false)
            return;

        foreach (var item in command.UpdateTypes)
        {
            var couldParse = Enum.TryParse<PlayerPropertyName>(item.Key, out var contract);
            if (couldParse && _contractType.Contains(contract))
            {
                await brandServiceClient
                    .SetBrandPlayerContractDisplayAsync(command.BrandId, item.Key, item.Value, command.Modifier, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
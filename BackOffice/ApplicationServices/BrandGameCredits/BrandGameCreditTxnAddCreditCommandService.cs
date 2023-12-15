using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

internal class BrandGameCreditTxnAddCreditCommandService(
    IBrandServiceClient brandServiceClient,
    IBrandGameCreditsClient client) : ICommandService<BrandGameCreditTxnAddCreditCommand>
{
    public async ValueTask ExecuteAsync(BrandGameCreditTxnAddCreditCommand command, CancellationToken cancellationToken = default)
    {
        var brandInfo = await brandServiceClient.GetBrandInfoAsync(
            brandId: command.BrandId,
            cancellationToken: cancellationToken)
            ?? throw new NullReferenceException($"This brand does not exist.{command.BrandId}");

        await client.GameCreditTxnAddCreditAsync(
            brandCode: brandInfo.Code,
            money: command.Money,
            amount: command.Amount,
            remark: command.Remark,
            expansions: command.Expansions,
            createdUserId: command.CreatedUserId,
            createdUsername: command.CreatedUsername,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}

#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

internal class BrandGameCreditGetCurrentGetService(
    IBrandServiceClient brandServiceClient,
    IBrandGameCreditsClient client) : IGetService<BrandGameCreditGetCurrentQuery, BrandGameCreditResult?>
{
    public async ValueTask<BrandGameCreditResult?> GetAsync(BrandGameCreditGetCurrentQuery query, CancellationToken cancellationToken = default)
    {
        var brandInfo = await brandServiceClient.GetBrandInfoAsync(
            brandId: query.BrandId,
            cancellationToken: cancellationToken)
            ?? throw new NullReferenceException($"This brand does not exist.{query.BrandId}");

        var gameCredit = await client.GameCreditGetCurrentAsync(
            brandCode: brandInfo.Code,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return gameCredit is null
            ? null
            : new BrandGameCreditResult(
                ResellerId: gameCredit.ResellerId,
                ResellerCode: gameCredit.ResellerCode,
                LicenseeId: gameCredit.LicenseeId,
                LicenseeCode: gameCredit.LicenseeCode,
                BrandId: gameCredit.BrandId,
                BrandCode: gameCredit.BrandCode,
                Month: gameCredit.Month,
                InitialAmount: gameCredit.InitialAmount,
                CreditAmount: gameCredit.CreditAmount,
                DebitAmount: gameCredit.DebitAmount,
                ProfitAmount: gameCredit.ProfitAmount,
                CreditLimit: gameCredit.CreditLimit,
                CreditUsed: gameCredit.CreditUsed,
                CreditUsedPer: gameCredit.CreditUsedPer,
                CreditAvailable: gameCredit.CreditAvailable,
                CreditAvailablePer: gameCredit.CreditAvailablePer,
                CreditRiskType: gameCredit.CreditRiskType,
                CreatedOn: gameCredit.CreatedOn,
                UpdatedOn: gameCredit.UpdatedOn);
    }
}

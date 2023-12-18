using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common.Models;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

internal class BrandGameCreditHistoryQueryService(
    IBrandServiceClient brandServiceClient,
    IBrandGameCreditsClient client) : IQueryService<BrandGameCreditHistoryQuery, BrandGameCreditResult>
{
    public async IAsyncEnumerable<BrandGameCreditResult> QueryAsync(BrandGameCreditHistoryQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var brandInfo = await brandServiceClient.GetBrandInfoAsync(
            brandId: query.BrandId,
            cancellationToken: cancellationToken)
            ?? throw new NullReferenceException($"This brand does not exist.{query.BrandId}");

        var gameCredits = await client.GameCreditHistoriesAsync(
            brandCode: brandInfo.Code,
            cancellationToken: cancellationToken)
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? Enumerable.Empty<BrandGameCredit>();

        if (!gameCredits.Any())
        {
            yield break;
        }

        foreach (var gameCredit in gameCredits)
        {
            yield return new BrandGameCreditResult(
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
}

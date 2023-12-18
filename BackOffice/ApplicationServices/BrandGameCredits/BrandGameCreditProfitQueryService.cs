using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Bibliography;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common.Models;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Models.GameProvider;

namespace TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

internal class BrandGameCreditProfitQueryService(
    IBrandServiceClient brandServiceClient,
    IBrandGameCreditsClient client) : IQueryService<BrandGameCreditProfitQuery, BrandGameCreditProfitResult>
{
    public async IAsyncEnumerable<BrandGameCreditProfitResult> QueryAsync(BrandGameCreditProfitQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var brandInfo = await brandServiceClient.GetBrandInfoAsync(
            brandId: query.BrandId,
            cancellationToken: cancellationToken)
            ?? throw new NullReferenceException($"This brand does not exist.{query.BrandId}");

        var gameCreditProfits = await client.GameCreditProfitQueryAsync(
            brandCode: brandInfo.Code,
            month: query.Month,
            cancellationToken: cancellationToken)
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? Enumerable.Empty<BrandGameCreditProfit>();

        if (!gameCreditProfits.Any())
        {
            yield break;
        }

        foreach (var gameCreditProfit in gameCreditProfits)
        {
            yield return new BrandGameCreditProfitResult(
                ResellerId: gameCreditProfit.ResellerId,
                ResellerCode: gameCreditProfit.ResellerCode,
                LicenseeId: gameCreditProfit.LicenseeId,
                LicenseeCode: gameCreditProfit.LicenseeCode,
                BrandId: gameCreditProfit.BrandId,
                BrandCode: gameCreditProfit.BrandCode,
                Month: gameCreditProfit.Month,
                GameProviderId: gameCreditProfit.GameProviderId,
                GameProviderCode: gameCreditProfit.GameProviderCode,
                GameProviderName: gameCreditProfit.GameProviderName,
                Count: gameCreditProfit.Count,
                Bet: gameCreditProfit.Bet,
                ValidBet: gameCreditProfit.ValidBet,
                Settle: gameCreditProfit.Settle,
                Commission: gameCreditProfit.Commission,
                Turnover: gameCreditProfit.Turnover,
                WinLoss: gameCreditProfit.WinLoss,
                ProfitAmount: gameCreditProfit.ProfitAmount);
        }
    }
}

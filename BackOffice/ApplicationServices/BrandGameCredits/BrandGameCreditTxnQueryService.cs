using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common.Models;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

internal class BrandGameCreditTxnQueryService(
    IBrandServiceClient brandServiceClient,
    IBrandGameCreditsClient client) : IQueryService<BrandGameCreditTxnQuery, BrandGameCreditTxnResult>, IGetService<BrandGameCreditTxnQuery, int>
{
    public async ValueTask<int> GetAsync(BrandGameCreditTxnQuery query, CancellationToken cancellationToken = default)
    {
        var brandInfo = await brandServiceClient.GetBrandInfoAsync(
            brandId: query.BrandId,
            cancellationToken: cancellationToken)
            ?? throw new NullReferenceException($"This brand does not exist.{query.BrandId}");

        return await client.GameCreditTxnQueryCountAsync(
            brandCode: brandInfo.Code,
            startOn: query.StartOn,
            endOn: query.EndOn,
            type: query.Type,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async IAsyncEnumerable<BrandGameCreditTxnResult> QueryAsync(BrandGameCreditTxnQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var brandInfo = await brandServiceClient.GetBrandInfoAsync(
            brandId: query.BrandId,
            cancellationToken: cancellationToken)
            ?? throw new NullReferenceException($"This brand does not exist.{query.BrandId}");

        var txns = await client.GameCreditTxnQueryAsync(
            brandCode: brandInfo.Code,
            startOn: query.StartOn,
            endOn: query.EndOn,
            type: query.Type,
            pageNumber: query.PageNumber,
            pageSize: query.PageSize,
            cancellationToken: cancellationToken)
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? Enumerable.Empty<BrandGameCreditTxn>();
        if (!txns.Any())
        {
            yield break;
        }

        foreach (var txn in txns)
        {
            yield return new BrandGameCreditTxnResult(
                Id: txn.Id,
                ResellerId: txn.ResellerId,
                ResellerCode: txn.ResellerCode,
                LicenseeId: txn.LicenseeId,
                LicenseeCode: txn.LicenseeCode,
                BrandId: txn.BrandId,
                BrandCode: txn.BrandCode,
                Type: txn.Type,
                Money: txn.Money,
                Amount: txn.Amount,
                Remark: txn.Remark,
                Expansions: txn.Expansions,
                CreatedUserId: txn.CreatedUserId,
                CreatedUsername: txn.CreatedUsername,
                CreatedOn: txn.CreatedOn);
        }
    }
}

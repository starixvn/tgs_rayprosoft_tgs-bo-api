using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Tgs.CashService.Client;
using Tgs.PlayerBucket.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.ApplicationServices.Players;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

internal class ProcessCleanBucketsRequestCommandService(
    IPlayerBucketClient playerBucketClient,
    IPlayerBucketForTypesClient playerBucketForTypesClient,
    ICashServiceClient cashServiceClient,
    IQueryService<GetPlayerGameProviderBalanceQuery, GetPlayerGameProviderBalanceResponse> gameProviderQueryService,
    ILogger<ProcessCleanBucketsRequestCommandService> logger)
    : ICommandService<ProcessCleanBucketsRequestCommand>
{
    public async ValueTask ExecuteAsync(ProcessCleanBucketsRequestCommand command, CancellationToken cancellationToken = default)
    {
        var form = await playerBucketClient.GetCleanBucketFormAsync(
            command.FormId,
            cancellationToken).ConfigureAwait(false);

        if (form is null)
            return;

        var bucketTypes = await playerBucketClient.GetBrandBucketTypesAsync(
            form.BrandId,
            cancellationToken)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var processingBucketResults = await playerBucketForTypesClient.ReadBucketsForBucketTypesAsync(
            form.PlayerCode,
            bucketTypes,
            cancellationToken)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var processingWalletBalance = await cashServiceClient.GetBalanceAsync(
            form.WalletId,
            cancellationToken).ConfigureAwait(false);

        var playerAllBalances = await gameProviderQueryService
            .QueryAsync(
                new GetPlayerGameProviderBalanceQuery(form.PlayerCode),
                cancellationToken)
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (!playerAllBalances.Where(x => x.IsMaintain).Any())
        {
            var gamesBalance = playerAllBalances.Sum(x => x.Balance) ?? 0M;

            if (form.CurrentBalance + gamesBalance <= form.ClearWageringRequirementBalance)
            {
                await Policy.Handle<Exception>()
                    .RetryAsync(2, (ex, count) =>
                    {
                        logger.LogWarning(ex, "Clean buckets throw exception, retry {count}.", count);
                    })
                    .ExecuteAsync(async () => await playerBucketForTypesClient
                        .CleanBucketsForBucketTypesAsync(
                            form.PlayerCode,
                            form.CurrentBucketAmounts
                                .Where(result => result.Result.AnchorBucketId is not null)
                                .Select(result => (result.BucketType, result.Result.AnchorBucketId.Value)),
                            form.DepositFormId.ToString(),
                            null,
                            cancellationToken)
                        .ConfigureAwait(false))
                    .ConfigureAwait(false);

                await playerBucketClient.CompleteCleanBucketsAsync(
                    form.Id,
                    new ReviewCleanBucketRequest<GetPlayerGameProviderBalanceResponse[]>(
                        processingWalletBalance,
                        processingBucketResults.Select(
                            result => new TypeBucketResult(result.BucketType, result.Result)),
                        playerAllBalances),
                    cancellationToken).ConfigureAwait(false);
            }
            else
                await playerBucketClient.RejectCleanBucketsAsync(
                    form.Id,
                    new ReviewCleanBucketRequest<GetPlayerGameProviderBalanceResponse[]>(
                        processingWalletBalance,
                        processingBucketResults.Select(
                            result => new TypeBucketResult(result.BucketType, result.Result)),
                        playerAllBalances),
                    cancellationToken).ConfigureAwait(false);
        }
    }
}

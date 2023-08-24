using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Tgs.CashService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.Player;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

internal class ProcessCleanBucketsRequestCommandService : ICommandService<ProcessCleanBucketsRequestCommand>
{
    private readonly ICashServiceClient _cashServiceClient;
    private readonly IQueryService<GetPlayerGameProviderBalanceQuery, GetPlayerGameProviderBalanceResponse> _gameProviderQueryService;
    private readonly ILogger<ProcessCleanBucketsRequestCommandService> _logger;
    private readonly IPlayerBucketClient _playerBucketClient;

    public ProcessCleanBucketsRequestCommandService(
        IPlayerBucketClient playerBucketClient,
        ICashServiceClient cashServiceClient,
        IQueryService<GetPlayerGameProviderBalanceQuery, GetPlayerGameProviderBalanceResponse> gameProviderQueryService,
        ILogger<ProcessCleanBucketsRequestCommandService> logger)
    {
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
        _gameProviderQueryService = gameProviderQueryService ?? throw new ArgumentNullException(nameof(gameProviderQueryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask ExecuteAsync(ProcessCleanBucketsRequestCommand command, CancellationToken cancellationToken = default)
    {
        var form = await _playerBucketClient.GetCleanBucketFormAsync(
            command.FormId,
            cancellationToken).ConfigureAwait(false);

        if (form is null)
            return;

        var processingBucketResult = await _playerBucketClient.ReadBucketsAsync(
            form.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        var processingWalletBalance = await _cashServiceClient.GetBalanceAsync(
            form.WalletId,
            cancellationToken).ConfigureAwait(false);

        var playerAllBalances = await _gameProviderQueryService
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
                        _logger.LogWarning(ex, "Clean buckets throw exception, retry {count}.", count);
                    })
                    .ExecuteAsync(async () => await _playerBucketClient
                        .CleanBucketsAsync(
                            form.PlayerCode,
                            form.CurrentBucketAmounts
                                .Where(result => result.BucketType is null)
                                .First()
                                .Result
                                .AnchorBucketId
                                .Value,
                            form.DepositFormId.ToString(),
                            null,
                            cancellationToken)
                        .ConfigureAwait(false))
                    .ConfigureAwait(false);

                await _playerBucketClient.CompleteCleanBucketsAsync(
                    form.Id,
                    new ReviewCleanBucketRequest<GetPlayerGameProviderBalanceResponse[]>(
                        processingWalletBalance,
                        new[]
                        {
                            new CleanFormBucketResult(
                                null,
                                processingBucketResult)
                        },
                        playerAllBalances),
                    cancellationToken).ConfigureAwait(false);
            }
            else
                await _playerBucketClient.RejectCleanBucketsAsync(
                    form.Id,
                    new ReviewCleanBucketRequest<GetPlayerGameProviderBalanceResponse[]>(
                        processingWalletBalance,
                        new[]
                        {
                            new CleanFormBucketResult(
                                null,
                                processingBucketResult)
                        },
                        playerAllBalances),
                    cancellationToken).ConfigureAwait(false);
        }
    }
}

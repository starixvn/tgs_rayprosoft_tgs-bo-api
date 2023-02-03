using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using StackExchange.Redis;
using Tgs.Payment.Clients;
using Tgs.Promotion.Clients;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using TGS.Core.Infrastructures.Redis;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Application.Queries.PlayerHandlers;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.PlayerBucket;
using TGS.Services.PlayerBucket.Models;

namespace TGS.Services.BackOffice.ApplicationServices.Payment;

internal class ReviewDepositFormCommandService : ICommandService<ReviewDepositFormCommand>
{
    private readonly IBrandService _brandService;
    private readonly IQueryService<GetPlayerGameProviderBalanceCommand, GetPlayerGameProviderBalanceResponse> _getPlayerGameProviderBalanceService;
    private readonly ILogger<ReviewDepositFormCommandService> _logger;
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IPlayerBucketRepository _playerBucketRepository;
    private readonly IPlayerBucketClient _playerBucketClient;
    private readonly IPlayerService _playerService;
    private readonly IPromotionClient _promotionClient;
    private readonly IDatabase _redisDatabase;

    public ReviewDepositFormCommandService(
        IPaymentServiceClient paymentServiceClient,
        IPlayerService playerService,
        IPlayerBucketRepository playerBucketRepository,
        IPlayerBucketClient playerBucketClient,
        IPromotionClient promotionClient,
        IRedisConnectionFactory redisConnectionFactory,
        IBrandService brandService,
        IQueryService<GetPlayerGameProviderBalanceCommand, GetPlayerGameProviderBalanceResponse> getPlayerGameProviderBalanceService,
        ILogger<ReviewDepositFormCommandService> logger)
    {
        if (redisConnectionFactory is null)
            throw new ArgumentNullException(nameof(redisConnectionFactory));
        _paymentServiceClient =
            paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));

        _playerBucketRepository =
            playerBucketRepository ?? throw new ArgumentNullException(nameof(playerBucketRepository));
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
        _promotionClient = promotionClient ?? throw new ArgumentNullException(nameof(promotionClient));
        _redisDatabase = redisConnectionFactory.GetConnection().GetDatabase();
        _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
        _getPlayerGameProviderBalanceService = getPlayerGameProviderBalanceService ?? throw new ArgumentNullException(nameof(getPlayerGameProviderBalanceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask ExecuteAsync(
        ReviewDepositFormCommand command,
        CancellationToken cancellationToken = default)
    {
        var isSaveSuccess = false;
        try
        {
            var form = await _paymentServiceClient
                .GetDepositFormAsync(command.BrandId, command.FormId, cancellationToken)
                .ConfigureAwait(false);

            if (form == null
                || form.Status == PaymentFlowState.Approved
                || form.Status == PaymentFlowState.Reject)
                return;

            isSaveSuccess = await _redisDatabase
                .StringSetAsync(
                    $"Tgs:BucketDeposit:{command.FormId:N}",
                    command.FormId.ToString(),
                    expiry: TimeSpan.FromMinutes(1),
                    when: When.NotExists)
                .ConfigureAwait(false);

            if (!isSaveSuccess)
                return;

            if (command.ActualAmount != null)
            {
                var handlingFee = await _paymentServiceClient
                    .CalculateHandlingFeeAsync(form.BrandPaymentId, command.ActualAmount.Value, cancellationToken)
                    .ConfigureAwait(false);

                await _paymentServiceClient
                    .SubmitDepositFormStatusAsync(
                        form.Id,
                        actualAmount: command.ActualAmount,
                        handlingFee: handlingFee,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            if (command.Status == ReviewState.Approve)
            {
                var brand = await _brandService.GetBrandAsync(x => x.Id == form.BrandId);

                if (brand.AutoClearWageringRequirement)
                {
                    try
                    {
                        var bucketInfo = await _playerBucketRepository.GetPlayerBucketInfoAsync(
                            form.PlayerCode,
                            cancellationToken).ConfigureAwait(false);

                        var playerAllBalances = await _getPlayerGameProviderBalanceService
                            .QueryAsync(
                                query: new GetPlayerGameProviderBalanceCommand(form.PlayerCode),
                                cancellationToken: cancellationToken)
                            .ToArrayAsync(cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

                        if (!playerAllBalances.Where(x => x.IsMaintain).Any()
                            && bucketInfo.Amount > 0M)
                        {
                            var balance = playerAllBalances.Sum(x => x.Balance) ?? 0M;

                            if (balance <= brand.AutoClearWageringRequirementBalance)
                                await Policy.Handle<Exception>()
                                    .RetryAsync(2, (ex, count) =>
                                    {
                                        _logger.LogWarning(ex, "Clean buckets throw exception, retry {count}.", count);
                                    })
                                    .ExecuteAsync(async () => await _playerBucketClient
                                        .CleanBucketsAsync(
                                            form.PlayerCode,
                                            bucketInfo.SpanshotId!.Value,
                                            form.Id.ToString(),
                                            null,
                                            cancellationToken)
                                        .ConfigureAwait(false))
                                    .ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "ClearWagering has error.");
                    }
                }
            }

            await _paymentServiceClient
                .ReviewDepositFormAsync(
                    command.FormId,
                    command.Status,
                    command.ReviewUserId,
                    command.ReviewUserName,
                    command.Remark,
                    cancellationToken)
                .ConfigureAwait(false);

            if (command.Status == ReviewState.Approve)
            {
                var player = await _playerService
                    .GetPlayerAsync(x => x.PlayerCode == form.PlayerCode)
                    .ConfigureAwait(false);

                var handlingFee = command.ActualAmount != null
                    ? await _paymentServiceClient
                        .CalculateHandlingFeeAsync(form.BrandPaymentId, command.ActualAmount.Value, cancellationToken)
                        .ConfigureAwait(false)
                    : await _paymentServiceClient
                        .CalculateHandlingFeeAsync(form.BrandPaymentId, form.Amount, cancellationToken)
                        .ConfigureAwait(false);

                var promotionResponse = await _playerBucketRepository
                    .CreatePlayerBucketWithDepositAsync(new CreatePlayerBucketWithDepositRequest(
                        command.BrandId,
                        form.PromotionId,
                        player.PlayerCode,
                        player.DefaultWalletId,
                        (command.ActualAmount ?? form.Amount) - handlingFee,
                        command.Remark,
                        command.FormId,
                        string.IsNullOrEmpty(command.ReviewUserId) ? null : Guid.Parse(command.ReviewUserId)))
                    .ConfigureAwait(false);

                await _paymentServiceClient
                    .SubmitDepositFormStatusAsync(
                        command.FormId,
                        promotionPlayerId: promotionResponse.PromotionPlayerId,
                        handlingFee: handlingFee,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                await _playerService
                    .TopUpAccumulatedDepositAsync(
                        form.PlayerCode,
                        (command.ActualAmount ?? form.Amount) - handlingFee)
                    .ConfigureAwait(false);

                if (form.PromotionId != null)
                    _ = await _promotionClient
                        .DepositJoinAsync(form.PromotionId.Value, form.PlayerCode)
                        .ConfigureAwait(false);
            }
            else
            {
                if (form.PromotionId != null)
                    await _promotionClient
                        .RevertPromotionAsync(form.PromotionId.Value, form.Amount)
                        .ConfigureAwait(false);
            }
        }
        finally
        {
            if (isSaveSuccess)
                _ = await _redisDatabase
                    .KeyDeleteAsync($"Tgs:BucketDeposit:{command.FormId:N}")
                    .ConfigureAwait(false);
        }
    }
}

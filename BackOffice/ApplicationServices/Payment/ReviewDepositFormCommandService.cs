#nullable enable

using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Tgs.BrandCertificate.Clients;
using Tgs.CashService.Client;
using Tgs.Payment.Clients;
using Tgs.PlayerBucket.Client;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using TGS.Core.Infrastructures.Redis;

namespace TGS.Services.BackOffice.ApplicationServices.Payment;

internal class ReviewDepositFormCommandService : ICommandService<ReviewDepositFormCommand>
{
    private readonly IBrandServiceClient _brandServiceClient;
    private readonly ICashServiceClient _cashServiceClient;
    private readonly ILogger<ReviewDepositFormCommandService> _logger;
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IPlayerBucketClient _playerBucketClient;
    private readonly IPlayerBucketForTypesClient _playerBucketForTypesClient;
    private readonly IPlayerServiceClient _playerServiceClient;
    private readonly IPromotionClient _promotionClient;
    private readonly IDatabase _redisDatabase;

    public ReviewDepositFormCommandService(
        IPaymentServiceClient paymentServiceClient,
        IPlayerServiceClient playerServiceClient,
        IPlayerBucketClient playerBucketClient,
        IPlayerBucketForTypesClient playerBucketForTypesClient,
        IPromotionClient promotionClient,
        IRedisConnectionFactory redisConnectionFactory,
        IBrandServiceClient brandServiceClient,
        ICashServiceClient cashServiceClient,
        ILogger<ReviewDepositFormCommandService> logger)
    {
        ArgumentNullException.ThrowIfNull(redisConnectionFactory);

        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
        _playerBucketForTypesClient = playerBucketForTypesClient ?? throw new ArgumentNullException(nameof(playerBucketForTypesClient));
        _promotionClient = promotionClient ?? throw new ArgumentNullException(nameof(promotionClient));
        _redisDatabase = redisConnectionFactory.GetConnection().GetDatabase();
        _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask ExecuteAsync(
        ReviewDepositFormCommand command,
        CancellationToken cancellationToken = default)
    {
        var form = await _paymentServiceClient
            .GetDepositFormAsync(command.FormId, cancellationToken)
            .ConfigureAwait(false);

        if (form is null
            || form.BrandId != command.BrandId
            || form.Status == PaymentFlowState.Approved
            || form.Status == PaymentFlowState.Reject)
            return;

        var isSaveSuccess = false;
        try
        {
            isSaveSuccess = await _redisDatabase.StringSetAsync(
                $"Tgs:BucketDeposit:{command.FormId:N}",
                command.FormId.ToString(),
                expiry: TimeSpan.FromMinutes(1),
                when: When.NotExists).ConfigureAwait(false);

            if (!isSaveSuccess)
                return;

            await ProcessDepositAsync(
                command,
                form,
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (isSaveSuccess)
                _ = await _redisDatabase
                    .KeyDeleteAsync($"Tgs:BucketDeposit:{command.FormId:N}")
                    .ConfigureAwait(false);
        }
    }

    private async ValueTask<int?> CreatePlayerBucketWithDepositAsync(
        ReviewDepositFormCommand command,
        DepositFormInfo form,
        decimal handlingFee,
        CancellationToken cancellationToken)
    {
        var depositAmount = (command.ActualAmount ?? form.Amount) - handlingFee;

        var playerProps = await _playerServiceClient.GetPlayerPropertiesAsync(
            form.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        var walletId = Guid.Parse(playerProps["DefaultWalletId"]);

        _ = await _playerBucketClient.CreateBucketAsync(
            form.PlayerCode,
            depositAmount,
            BucketType.Deposit,
            form.Id.ToString(),
            command.Remark,
            cancellationToken).ConfigureAwait(false);

        _ = await _cashServiceClient.DepositedAsync(
            walletId,
            depositAmount,
            command.Remark,
            form.Id.ToString(),
            string.IsNullOrEmpty(command.ReviewUserId) ? null : Guid.Parse(command.ReviewUserId),
            cancellationToken).ConfigureAwait(false);

        return null;
    }

    private async Task ProcessDepositAsync(ReviewDepositFormCommand command, DepositFormInfo form, CancellationToken cancellationToken)
    {
        var playerProps = await _playerServiceClient.GetPlayerPropertiesAsync(
            form.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        var walletId = Guid.Parse(playerProps["DefaultWalletId"]);

        if (command.Status == ReviewState.Approve)
        {
            var brandPaymentSettings = await _brandServiceClient.GetBrandPaymentSettingsAsync(
                form.BrandId,
                cancellationToken).ConfigureAwait(false);

            if (brandPaymentSettings?.AutoClearWageringRequirement == true)
            {
                try
                {
                    var bucketTypes = await _playerBucketClient.GetBrandBucketTypesAsync(
                        command.BrandId,
                        cancellationToken)
                        .ToArrayAsync(cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                    var bucketResults = await _playerBucketForTypesClient.ReadBucketsForBucketTypesAsync(
                        form.PlayerCode,
                        bucketTypes,
                        cancellationToken)
                        .Where(result => result.Result.Amount > 0M)
                        .ToArrayAsync(cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    var walletBalance = await _cashServiceClient.GetBalanceAsync(
                        walletId,
                        cancellationToken).ConfigureAwait(false);

                    if (walletBalance <= brandPaymentSettings.ClearWageringRequirementBalance
                        && bucketResults.Length > 0)
                    {
                        await _playerBucketClient.RequestCleanBucketsAsync(
                            form.PlayerCode,
                            new CleanBucketOrder(
                                form.BrandId,
                                walletId,
                                walletBalance,
                                bucketResults.Select(result => new TypeBucketResult(
                                    result.BucketType,
                                    result.Result)),
                                form.Id,
                                brandPaymentSettings.ClearWageringRequirementBalance),
                            cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Clean Buckets occur error.");
                }
            }
        }

        var handlingFee = await _paymentServiceClient
            .CalculateHandlingFeeAsync(
                form.BrandPaymentId,
                command.ActualAmount ?? form.Amount,
                cancellationToken)
            .ConfigureAwait(false);

        await _paymentServiceClient
            .SubmitDepositFormStatusAsync(
                form.Id,
                actualAmount: command.ActualAmount,
                handlingFee: handlingFee,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await _paymentServiceClient.ReviewDepositFormAsync(
            command.FormId,
            command.Status,
            command.ReviewUserId,
            command.ReviewUserName,
            command.Remark,
            cancellationToken).ConfigureAwait(false);

        if (command.Status == ReviewState.Approve)
        {
            var promotionPlayerId = await CreatePlayerBucketWithDepositAsync(
                command,
                form,
                handlingFee,
                cancellationToken)
                .ConfigureAwait(false);

            await _paymentServiceClient.SubmitDepositFormStatusAsync(
                command.FormId,
                promotionPlayerId: promotionPlayerId,
                handlingFee: handlingFee,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            await _playerServiceClient.TopUpAccumulatedDepositAsync(
                form.PlayerCode,
                (command.ActualAmount ?? form.Amount) - handlingFee,
                cancellationToken).ConfigureAwait(false);

            if (form.PromotionId is not null)
                _ = await _promotionClient
                    .DepositJoinAsync(form.PromotionId.Value, form.PlayerCode)
                    .ConfigureAwait(false);
        }
        else
        {
            if (form.PromotionId is not null)
                await _promotionClient
                    .RevertPromotionAsync(form.PromotionId.Value, form.Amount)
                    .ConfigureAwait(false);
        }
    }

    private record CreatePlayerBucketWithDepositRequest(
        Guid BrandId,
        Guid? PromotionId,
        string PlayerCode,
        Guid DefaultWalletId,
        decimal DepositAmount,
        string Remark,
        Guid FormId,
        Guid? UserId);
    private record DepositMessageDataResponse(int? PromotionBonusPlayerId, decimal DepositBonusAmount);
}

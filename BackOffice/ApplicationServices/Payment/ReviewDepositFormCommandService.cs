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
using Tgs.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using TGS.Core.Infrastructures.Redis;
using TGS.Services.BackOffice.Application.Models.Player;

namespace TGS.Services.BackOffice.ApplicationServices.Payment;

internal class ReviewDepositFormCommandService : ICommandService<ReviewDepositFormCommand>
{
    private static readonly string[] _AllowDistributePromotionTypes = new[] { "FirstDeposit", "DailySecondDeposit", "DailyFirstDeposit", "DepositReward" };
    private readonly IBrandServiceClient _brandServiceClient;
    private readonly ICashServiceClient _cashServiceClient;
    private readonly ILogger<ReviewDepositFormCommandService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IPlayerBucketClient _playerBucketClient;
    private readonly IPlayerServiceClient _playerServiceClient;
    private readonly IPromotionClient _promotionClient;
    private readonly IPromotionServiceClient _promotionServiceClient;
    private readonly IDatabase _redisDatabase;

    public ReviewDepositFormCommandService(
        IPaymentServiceClient paymentServiceClient,
        IPlayerServiceClient playerServiceClient,
        IPlayerBucketClient playerBucketClient,
        IPromotionClient promotionClient,
        IPromotionServiceClient promotionServiceClient,
        IRedisConnectionFactory redisConnectionFactory,
        IBrandServiceClient brandServiceClient,
        ICashServiceClient cashServiceClient,
        IMessageSender messageSender,
        ILogger<ReviewDepositFormCommandService> logger)
    {
        if (redisConnectionFactory is null)
            throw new ArgumentNullException(nameof(redisConnectionFactory));

        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
        _promotionClient = promotionClient ?? throw new ArgumentNullException(nameof(promotionClient));
        _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
        _redisDatabase = redisConnectionFactory.GetConnection().GetDatabase();
        _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask ExecuteAsync(
        ReviewDepositFormCommand command,
        CancellationToken cancellationToken = default)
    {
        var form = await _paymentServiceClient
            .GetDepositFormAsync(command.BrandId, command.FormId, cancellationToken)
            .ConfigureAwait(false);

        if (form is null
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

        _ = await _cashServiceClient.DepositedAsync(
            walletId,
            depositAmount,
            command.Remark,
            form.Id.ToString(),
            string.IsNullOrEmpty(command.ReviewUserId) ? null : Guid.Parse(command.ReviewUserId),
            cancellationToken).ConfigureAwait(false);

        var promotion = form.PromotionId is null
            ? null
            : await _promotionServiceClient.GetPromotionMainInfoAsync(
                form.PromotionId.Value,
                cancellationToken).ConfigureAwait(false);

        if (promotion is null || promotion.PromotionType == "DepositAccumulateRoulette")
            _ = await _playerBucketClient.CreateBucketAsync(
                form.PlayerCode,
                depositAmount,
                BucketType.Deposit,
                form.Id.ToString(),
                command.Remark,
                cancellationToken).ConfigureAwait(false);
        else if (_AllowDistributePromotionTypes.Contains(promotion.PromotionType))
        {
            var subject = $"tgs.promotion.{promotion?.PromotionType.ToLower()}.distribute";
            var request = new CreatePlayerBucketWithDepositRequest(
                command.BrandId,
                form.PromotionId,
                form.PlayerCode,
                (command.ActualAmount ?? form.Amount) - handlingFee,
                command.Remark,
                command.FormId,
                string.IsNullOrEmpty(command.ReviewUserId) ? null : Guid.Parse(command.ReviewUserId));

            var response = await _messageSender.RequestAsync(
                subject,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var json = Encoding.UTF8.GetString(response.Span);
            var responseData = JsonSerializer.Deserialize<DepositMessageDataResponse>(json)!;

            return responseData.PromotionBonusPlayerId;
        }

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
                    var bucketResult = await _playerBucketClient.ReadBucketsAsync(
                        form.PlayerCode,
                        cancellationToken).ConfigureAwait(false);

                    var walletBalance = await _cashServiceClient.GetBalanceAsync(
                        walletId,
                        cancellationToken).ConfigureAwait(false);

                    if (walletBalance <= brandPaymentSettings.ClearWageringRequirementBalance
                        && bucketResult.Amount > 0M)
                    {
                        await _playerBucketClient.RequestCleanBucketsAsync(
                            form.PlayerCode,
                            new CleanBucketOrder(
                                form.BrandId,
                                walletId,
                                walletBalance,
                                bucketResult.Amount,
                                bucketResult.AnchorBucketId!.Value,
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
        decimal DepositAmount,
        string Remark,
        Guid FormId,
        Guid? UserId);
    private record DepositMessageDataResponse(int? PromotionBonusPlayerId, decimal DepositBonusAmount);
}

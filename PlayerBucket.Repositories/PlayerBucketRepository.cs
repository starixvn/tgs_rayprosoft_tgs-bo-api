using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.CashService.Client;
using Tgs.MessageQueue;
using Tgs.Service.PlayerBuckets;
using TGS.Core.DataServices.Tgs;
using TGS.Services.PlayerBucket;
using TGS.Services.PlayerBucket.Models;

#nullable enable

namespace PlayerBucket.Repositories;

public class PlayerBucketRepository : IPlayerBucketRepository
{
    private readonly ICashServiceClient _cashServiceClient;
    private readonly IMessageSender _messageSender;
    private readonly IPlayerBucketClient _playerBucketClient;
    private readonly ITgsRepository _repository;

    public PlayerBucketRepository(
        IMessageSender messageSender,
        ITgsRepository repository,
        ICashServiceClient cashServiceClient,
        IPlayerBucketClient playerBucketClient)
    {
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
    }

    public ValueTask ClearPlayerBucketsAsync(
        string playerCode,
        Guid anchorId,
        string? externalId,
        CancellationToken cancellationToken = default)
        => _playerBucketClient.CleanBucketsAsync(
            playerCode,
            anchorId,
            externalId,
            cancellationToken);

    public async Task CreatePlayerBucketsAsync(CreatePlayerBucketRequest[] requests)
    {
        foreach (var request in requests)
            _ = await _playerBucketClient.CreateBucketAsync(
                request.PlayerCode,
                request.BucketAmount,
                request.BucketType,
                request.ExternalId).ConfigureAwait(false);
    }

    public async ValueTask<CreatePlayerBucketWithDepositResponse> CreatePlayerBucketWithDepositAsync(
        CreatePlayerBucketWithDepositRequest request)
    {
        var depositBonusAmount = request.DepositAmount;
        var promotionPlayerId = default(int?);

        await _cashServiceClient.DepositedAsync(
            request.DefaultWalletId,
            request.DepositAmount,
            request.Remark,
            request.FormId.ToString(),
            request.UserId).ConfigureAwait(false);

        if (request.PromotionId != null)
        {
            var promotion = await _repository.Promotions.FirstAsync(x => x.Id == request.PromotionId);

            var allowDistributePromotionTypes = new string[] { "FirstDeposit", "DailySecondDeposit", "DailyFirstDeposit", "DepositReward" };
            if (allowDistributePromotionTypes.Contains(promotion.PromotionType))
            {
                var subject = $"tgs.promotion.{promotion.PromotionType.ToLower()}.distribute";

                var response = await _messageSender.RequestAsync(subject, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)))
                    .ConfigureAwait(false);

                var json = Encoding.UTF8.GetString(response.Span);
                var responseData = JsonSerializer.Deserialize<DepositMessageDataResponse>(json)!;

                promotionPlayerId = responseData.PromotionBonusPlayerId;
                depositBonusAmount += responseData.DepositBonusAmount;
            }
            else if (promotion.PromotionType == "DepositAccumulateRoulette")
            {
                _ = await _playerBucketClient.CreateBucketAsync(
                    request.PlayerCode,
                    request.DepositAmount,
                    BucketType.Deposit,
                    request.FormId.ToString()).ConfigureAwait(false);
            }
        }
        else
        {
            _ = await _playerBucketClient.CreateBucketAsync(
                request.PlayerCode,
                request.DepositAmount,
                BucketType.Deposit,
                request.FormId.ToString()).ConfigureAwait(false);
        }

        return new CreatePlayerBucketWithDepositResponse(depositBonusAmount, promotionPlayerId);
    }

    public async ValueTask<PlayerBucketInfo> GetPlayerBucketInfoAsync(string playerCode, CancellationToken cancellationToken = default)
    {
        var info = await _playerBucketClient.ReadBucketsAsync(
            playerCode,
            cancellationToken).ConfigureAwait(false);

        return new PlayerBucketInfo(info.Amount, info.AnchorBucketId);
    }
}

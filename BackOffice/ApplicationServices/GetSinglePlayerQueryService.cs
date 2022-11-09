#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices;

public class GetSinglePlayerQueryService : IGetService<GetSinglePlayerQuery, PlayerModel?>
{
    private readonly ICashServiceClient _cashServiceClient;
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IPlayerBucketClient _playerBucketClient;
    private readonly IPlayerServiceClient _playerServiceClient;
    private readonly IServiceCoordinator _serviceCoordinator;

    public GetSinglePlayerQueryService(
        IPlayerServiceClient playerServiceClient,
        IPaymentServiceClient paymentServiceClient,
        ICashServiceClient cashServiceClient,
        IPlayerBucketClient playerBucketClient,
        IServiceCoordinator serviceCoordinator)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
        _serviceCoordinator = serviceCoordinator ?? throw new ArgumentNullException(nameof(serviceCoordinator));
    }

    public async ValueTask<PlayerModel?> GetAsync(GetSinglePlayerQuery query, CancellationToken cancellationToken = default)
    {
        var player = await _playerServiceClient
            .GetPlayerInfoAsync(query.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (player is null)
            return null;

        var loginInfo = await _playerServiceClient.GetPlayerLoginInfoAsync(
            query.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        var parentPlayer = await _playerServiceClient.GetParentPlayerInfoAsync(
            player.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        var parentUsername = parentPlayer?.UserName ?? string.Empty;

        var playerProperties = await _playerServiceClient
            .GetPlayerPropertiesAsync(player.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        var bankInfos = await _paymentServiceClient
            .GetPlayerBankInfosAsync(player.PlayerCode, cancellationToken)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var playerVipLevel = player.VipLevelId is null
            ? default(int?)
            : await _serviceCoordinator
                .QueryAsync<AllVipLevelsQuery, VipLevelInfo>(new(player.BrandId), cancellationToken)
                .Where(vip => vip.Id == player.VipLevelId)
                .Select(vip => vip.Level)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

        var playerAgentId = playerProperties.GetValueOrDefault("AgentId");

        var agent = playerAgentId is not null
            ? await _serviceCoordinator.QueryAsync<AllAgentsByBrandQuery, AgentInfo>(new(player.BrandId), cancellationToken)
                .Where(a => a.AgentId == Guid.Parse(playerAgentId))
                .SingleAsync(cancellationToken)
                .ConfigureAwait(false)
            : null;

        var walletId = Guid.Parse(playerProperties.GetValueOrDefault("DefaultWalletId")!);

        var bucketInfo = await _playerBucketClient.ReadBucketsAsync(
            player.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        var playerBirthdate = playerProperties.GetValueOrDefault("Birthdate");

        var status = await _playerServiceClient.GetPlayerStatusAsync(
            player.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        return new PlayerModel
        {
            PlayerCode = player.PlayerCode,
            Username = player.UserName,
            RealName = playerProperties.GetValueOrDefault("RealName"),
            VipLevel = playerVipLevel,
            VipId = player.VipLevelId,
            Status = status,
            StatusName = status.ToString(),
            RegisteredOn = player.RegisteredOn,
            AgentUsername = agent?.UserName,
            Balance = await _cashServiceClient.GetBalanceAsync(
                walletId,
                cancellationToken).ConfigureAwait(false),
            AreaCode = playerProperties.GetValueOrDefault("AreaCode"),
            Phone = playerProperties.GetValueOrDefault("Phone"),
            Email = playerProperties.GetValueOrDefault("Email"),
            QQ = playerProperties.GetValueOrDefault("QQ"),
            WeChat = playerProperties.GetValueOrDefault("WeChat"),
            Line = playerProperties.GetValueOrDefault("Line"),
            Viber = playerProperties.GetValueOrDefault("Viber"),
            FacebookMessenger = playerProperties.GetValueOrDefault("FacebookMessenger"),
            Telegram = playerProperties.GetValueOrDefault("Telegram"),
            WhatsApp = playerProperties.GetValueOrDefault("WhatsApp"),
            Banks = bankInfos.Select(p => new PlayerModel.BankModel
            {
                Id = p.Id,
                PaymentChannelId = p.PaymentChannelId,
                BankType = p.BankType,
                Detail = p.Detail,
                IsDefault = p.IsDefault
            }).ToArray(),
            WageringRequirement = bucketInfo.Amount,
            RegisterIp = loginInfo?.RegisterIp ?? string.Empty,
            RegisterHost = loginInfo?.RegisterHost ?? string.Empty,
            LastLoginIp = loginInfo?.LoginIp ?? string.Empty,
            LastLoginOn = loginInfo?.LoginOn,
            IdNumber = playerProperties.GetValueOrDefault("IdNumber"),
            Remark = playerProperties.GetValueOrDefault("Remark"),
            ParentUsername = parentUsername,
            Birthdate = playerBirthdate is not null
                ? DateOnly.Parse(playerBirthdate).ToString("yyyy/MM/dd")
                : null
        };
    }
}

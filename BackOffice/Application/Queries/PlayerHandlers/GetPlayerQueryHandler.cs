using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using TGS.Core.DataServices.Tgs;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

public class GetPlayerQueryHandler : Handler<GetPlayerRequest, GetPlayerResponse>
{
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IPlayerIpService _playerIpService;
    private readonly IPlayerServiceClient _playerServiceClient;
    private readonly ITgsRepository _repository;
    private readonly IPlayerService _service;

    public GetPlayerQueryHandler(
        IPlayerService service,
        ITgsRepository repository,
        IHttpContextAccessor accessor,
        IPlayerIpService playerIpService,
        IPlayerServiceClient playerServiceClient,
        IPaymentServiceClient paymentServiceClient)
        : base(accessor)
    {
        _service = service;
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _playerIpService = playerIpService ?? throw new ArgumentNullException(nameof(playerIpService));
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public override async Task<GetPlayerResponse> Handle(
        GetPlayerRequest request,
        CancellationToken cancellationToken)
    {
        var player = await _service
            .GetPlayerAsync(p => p.PlayerCode == request.PlayerCode)
            .ConfigureAwait(false);

        if (player == null)
        {
            return CreateInvalidResponse<GetPlayerResponse>(Player.NotFound);
        }

        var playerBuckets = await _repository.PlayerBuckets.AsQueryable()
            .Where(x => x.PlayerCode == player.PlayerCode)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var registerIp = _playerIpService
            .GetPlayerIpRecordsByRegister(player.BrandId, player.Username)
            .FirstOrDefault();

        var loginIp = _playerIpService
            .GetPlayerIpRecordsByLogin(player.BrandId, player.Username)
            .FirstOrDefault();

        var playerParentQuery = await _repository.PlayerParents.AsQueryable()
            .Where(pp => pp.PlayerCode == request.PlayerCode)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var parentUsername = string.Empty;
        if (playerParentQuery != null)
        {
            var playerParent = await _service
                .GetPlayerAsync(p => p.PlayerCode == playerParentQuery.ParentPlayerCode)
                .ConfigureAwait(false);

            parentUsername = playerParent.Username;
        }

        var playerProperties = await _playerServiceClient
            .GetPlayerPropertiesAsync(player.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        var bankInfos = await _paymentServiceClient
            .GetPlayerBankInfosAsync(player.PlayerCode, cancellationToken)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new GetPlayerResponse
        {
            Valid = true,
            Player = new PlayerModel
            {
                PlayerCode = player.PlayerCode,
                Username = player.Username,
                RealName = player.RealName,
                VipLevel = player.Vip?.Level,
                VipId = player.Vip?.Id,
                Status = player.Status,
                StatusName = player.Status.ToString(),
                RegisteredOn = player.RegisteredOn,
                AgentUsername = player.Agent?.Username,
                Balance = player.Wallets.FirstOrDefault()?.Balance ?? 0,
                AreaCode = player.AreaCode,
                Phone = player.Phone,
                Email = player.Email,
                QQ = player.QQ,
                WeChat = player.Wechat,
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
                WageringRequirement = playerBuckets.Sum(p => p.BucketAmount)
                                      - playerBuckets.Sum(p => p.CurrentBucketAmount),
                RegisterIp = registerIp?.Ip ?? string.Empty,
                RegisterHost = registerIp?.Host ?? string.Empty,
                LastLoginIp = loginIp?.Ip ?? string.Empty,
                LastLoginOn = loginIp?.RecordOn,
                IdNumber = player.IdNumber,
                Remark = player.Remark,
                ParentUsername = parentUsername,
                Birthdate = player.Birthdate?.ToString("yyyy/MM/dd")
            }
        };
    }
}

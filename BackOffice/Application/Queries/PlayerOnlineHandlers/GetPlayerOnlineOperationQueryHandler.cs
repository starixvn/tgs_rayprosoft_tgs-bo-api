using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Services.BackOffice.Application.Models.PlayerOnline;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlayerOnlineHandlers;

public class GetPlayerOnlineOperationQueryHandler : IGetService<GetPlayerOnlineRequest, PlayerOnlineModel>
{
    private readonly IPlayerIpService _playerIpService;
    private readonly IPlayerOnlineRepository _playerOnlineRepository;
    private readonly IPlayerService _playerService;

    public GetPlayerOnlineOperationQueryHandler(
        IPlayerOnlineRepository playerOnlineRepository,
        IPlayerService playerService,
        IPlayerIpService playerIpService)
    {
        _playerOnlineRepository = playerOnlineRepository ?? throw new ArgumentNullException(nameof(playerOnlineRepository));
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
        _playerIpService = playerIpService ?? throw new ArgumentNullException(nameof(playerIpService));
    }

    public async ValueTask<PlayerOnlineModel> GetAsync(GetPlayerOnlineRequest query, CancellationToken cancellationToken = default)
    {
        var playerCodes = await _playerOnlineRepository
             .GetQueryAsync(query.BrandId)
             .ConfigureAwait(false);

        var playerIps = _playerIpService.GetPlayerIpRecordsByLogin(playerCodes);

        var datas = await _playerService.GetPlayerQueries(
            x => x.Where(x => playerCodes.Contains(x.PlayerCode))
            .Where(query.VipId.HasValue, x => x.Vip.Id == query.VipId.Value)
            .Where(query.Status.HasValue, x => x.Status == query.Status.Value)
            .Where(!string.IsNullOrEmpty(query.PlayerCode), x => x.PlayerCode == query.PlayerCode))
            .Join(playerIps,
                player => player.PlayerCode,
                playerIp => playerIp.PlayerCode,
                (player, playerIp) => new PlayerOnlineData()
                {
                    BrandId = player.BrandId,
                    PlayerCode = player.PlayerCode,
                    Username = player.Username,
                    RealName = player.RealName,
                    Status = player.Status,
                    VipId = player.Vip.Id,
                    VipLevel = player.Vip.Level,
                    LoginIp = playerIp.Ip,
                    LoginOn = playerIp.RecordOn
                })
            .CreatePagedResponseAsync(query)
            .ConfigureAwait(false);

        return new(playerCodes.Length, datas);
    }
}

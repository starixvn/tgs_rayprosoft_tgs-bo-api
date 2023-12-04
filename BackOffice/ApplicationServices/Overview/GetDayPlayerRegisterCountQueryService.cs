using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

internal class GetDayPlayerRegisterCountQueryService(IPlayerServiceClient playerServiceClient) : IGetService<GetDayPlayerRegisterCountQuery, DayPlayerRegisterCountInfo>
{
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));

    public async ValueTask<DayPlayerRegisterCountInfo> GetAsync(GetDayPlayerRegisterCountQuery query, CancellationToken cancellationToken = default)
    {
        var yesterday = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8d)).AddDays(-1);

        var todayData = await _playerServiceClient
            .CountCurrentDayRegisterPlayersAsync(query.BrandId, cancellationToken)
            .ConfigureAwait(false);

        var yesterdayData = await _playerServiceClient
            .CountReportDateRegisterPlayersAsync(query.BrandId, DateOnly.FromDateTime(yesterday.DateTime), cancellationToken)
            .ConfigureAwait(false);

        return new DayPlayerRegisterCountInfo(todayData, yesterdayData);
    }
}

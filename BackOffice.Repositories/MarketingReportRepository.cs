using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Dapper;
using TGS.Core.DataServices.Tgs;
using TGS.Services.BackOffice.Domain;

namespace TGS.BackOffice.Repositories;

internal class MarketingReportRepository : IMarketingReportRepository
{
    private readonly ITgsRepository _tgsRepository;

    public MarketingReportRepository(ITgsRepository tgsRepository)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
    }

    public async IAsyncEnumerable<PlayerDailyGamingItem> QueryRealTimePlayerDailyGamingStatisticsAsync(
        Guid brandId,
        DateOnly queryDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var reportDateTime = new DateTimeOffset(
            queryDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified),
            TimeSpan.FromHours(8D));

        var conn = _tgsRepository.GetDbConnection();

        var result = await conn.QueryAsync<dynamic>(
            @"
                SELECT
                    PGA.player_code AS PlayerCode,
                    P.username AS PlayerName,
                    PGA.agent_id AS AgentId,
                    GP.code AS GpCode,
                    G.sub_game_provider_code AS SubGpCode,
                    Sum(PGA.win_loss) AS WinLossSummary
                FROM
                    report.player_game_rebate_aggregates AS PGA
                    JOIN
                    dbo.game_providers AS GP ON PGA.game_provider_id = GP.id
                    JOIN
                    dbo.games AS G ON PGA.game_provider_id = G.game_provider_id
                        AND PGA.game_id = G.id
                    JOIN
                    dbo.players AS P ON PGA.player_code = P.player_code
                WHERE
                    PGA.brand_id = @BrandId
                    AND
                    PGA.report_date = @ReportDateTime
                    AND
                    P.agent_id IS NOT NULL
                GROUP BY
                    PGA.agent_id,
                    PGA.player_code,
                    P.username,
                    GP.code,
                    G.sub_game_provider_code,
                    PGA.game_id",
            new
            {
                BrandId = brandId,
                ReportDateTime = reportDateTime
            }).ConfigureAwait(false);

        foreach (var row in result
            .Select(row => new PlayerDailyGamingItem(
                row.PlayerCode,
                row.PlayerName,
                row.AgentId,
                row.GpCode,
                row.SubGpCode,
                row.WinLossSummary)))
            yield return row;
    }

    public async IAsyncEnumerable<PlayerDailyRegisterItem> QueryRealTimePlayerDailyRegisterStatisticsAsync(
            Guid brandId,
        DateOnly queryDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (From, To) = (queryDate.AddDays(-1), queryDate);
        var queryTimeRange = (
            From: new DateTimeOffset(
                From
                    .AddDays(-1)
                    .ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(16D)), DateTimeKind.Utc),
                TimeSpan.Zero),
            To: new DateTimeOffset(
                To
                    .AddDays(-1)
                    .ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(16D)), DateTimeKind.Utc),
                TimeSpan.Zero));

        var conn = _tgsRepository.GetDbConnection();

        var result = await conn.QueryAsync<dynamic>(
            @"
                SELECT
                    P.player_code AS PlayerCode,
                    P.username AS UserName,
                    P.agent_id AS AgentId,
                    P.registered_on AS RegisteredOn,
                    PIR.record_on AS LastLoginOn,
                    PLRC.login_count AS LoginCount
                FROM
                    dbo.players AS P
                    LEFT JOIN
                    dbo.player_ip_records AS PIR ON P.player_code = PIR.player_code
                        AND PIR.ip_type = 2
                    LEFT JOIN
                    (
                        SELECT
                            P1.player_code,
                            Count(1) AS login_count
                        FROM
                            dbo.players AS P1
                            JOIN
                            dbo.player_login_records AS PLR ON P1.player_code = PLR.player_code
                        WHERE
                            P1.brand_id = @BrandId
                            AND
                            P1.agent_id IS NOT NULL
                            AND
                            P1.registered_on >= @Begin
                            AND
                            P1.registered_on < @End
                        GROUP BY
                            P1.player_code
                    ) AS PLRC ON P.player_code = PLRC.player_code
                WHERE
                    P.brand_id = @BrandId
                    AND
                    P.agent_id IS NOT NULL
                    AND
                    P.registered_on >= @Begin
                    AND
                    P.registered_on < @End",
            new
            {
                BrandId = brandId,
                Begin = queryTimeRange.From,
                End = queryTimeRange.To
            }).ConfigureAwait(false);

        foreach (var row in result
            .Select(row => new PlayerDailyRegisterItem(
                row.PlayerCode,
                row.UserName,
                row.AgentId,
                row.RegisteredOn,
                row.LastLoginOn,
                row.LoginCount)))
            yield return row;
    }
}

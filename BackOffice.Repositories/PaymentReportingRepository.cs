using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Dapper;
using Microsoft.Extensions.Logging;
using TGS.Core.DataServices.Tgs;
using TGS.Services.BackOffice.Domain;

namespace TGS.BackOffice.Repositories;

internal class PaymentReportingRepository(ITgsRepository tgsRepository, ILogger<PaymentReportingRepository> logger) : IPaymentReportingRepository
{
    private readonly ILogger<PaymentReportingRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ITgsRepository _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));

    public IAsyncEnumerable<AgentPlayerDepositStatistics> QueryAgentDailyDepositStatisticsAsync(
        Guid brandId,
        DateOnly queryLatestDate,
        CancellationToken cancellationToken = default)
    {
        var (from, to) = (
            queryLatestDate.AddDays(-91),
            queryLatestDate);

        var query = _tgsRepository.AgentDailyPaymentStatistics.AsQueryable()
            .Where(row => row.BrandId == brandId
                && row.ReportDate >= from
                && row.ReportDate < to)
            .Select(row => new AgentPlayerDepositStatistics(
                row.ReportDate,
                row.AgentId,
                row.FirstDepositPlayers,
                row.DepositPlayers,
                row.UndepositAgainPlayers));

        return query
            .ToArray()
            .ToAsyncEnumerable();
    }

    public async IAsyncEnumerable<PlayerDailyPaymentItem> QueryPlayerDailyDepositStatisticsAsync(
        Guid brandId,
        DateOnly queryDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("query date: {queryDate}", queryDate);

        var conn = _tgsRepository.GetDbConnection();

        var result = await conn.QueryAsync<PlayerDailyPaymentItem>(
            @"
                WITH CTE(
                    player_code,
                    is_first_deposit,
                    un_second_deposit
                )
                AS
                (
                    SELECT
                        COALESCE(FD.player_code, SD.player_code) AS player_code,
                        FD.is_first_deposit,
                        CASE SD.player_code
                            WHEN NULL THEN false
                            ELSE true
                        END AS un_second_deposit
                    FROM
                        (
                            SELECT
                                PS.player_code,
                                CASE
                                    WHEN PS.report_date = (PS.first_deposit_on AT TIME ZONE 'Asia/Shanghai')::date THEN true
                                    ELSE false
                                END AS is_first_deposit
                            FROM
                                dbo.player_daily_payment_statistics AS PS
                            WHERE
                                PS.report_date = @ReportDate
                        ) AS FD
                        FULL OUTER JOIN
                        (
                            SELECT
                                player_code
                            FROM
                                dbo.player_daily_payment_statistics AS PS
                            WHERE
                                report_date <= @UndepositCheckDate
                            GROUP BY
                                player_code
                            HAVING
                                Max(total_deposit_count) = 1
                        ) AS SD ON FD.player_code = SD.player_code
                ),
                Info(
                    player_code,
                    first_deposit_on,
                    first_deposit_amount,
                    total_deposit_amount,
                    total_withdraw_amount
                )
                AS
                (
                    SELECT
                        PS.player_code,
                        Max(PS.first_deposit_on) AS first_deposit_on,
                        COALESCE(Max(PS.first_deposit_amount), 0) AS first_deposit_amount,
                        COALESCE(Max(PS.total_deposit_amount), 0) AS total_deposit_amount,
                        COALESCE(Max(PS.total_withdraw_amount), 0) AS total_withdraw_amount
                    FROM
                        dbo.player_daily_payment_statistics AS PS
                        JOIN
                        CTE ON PS.player_code = CTE.player_code
                    WHERE
                        PS.report_date <= @ReportDate
                    GROUP BY
                        PS.player_code
                )
                SELECT
                    CTE.player_code AS PlayerCode,
                    P.username AS PlayerName,
                    P.agent_id AS AgentId,
                    CTE.is_first_deposit AS IsFirstDeposit,
                    CTE.un_second_deposit AS UnSecondDeposit,
                    Info.first_deposit_on AS FirstDepositOn,
                    Info.first_deposit_amount AS FirstDepositAmount,
                    Info.total_deposit_amount AS TotalDepositAmount,
                    Info.total_withdraw_amount AS TotalWithdrawAmount,
                    PIR.ip AS RegisterIp
                FROM
                    CTE
                    JOIN
                    INFO ON CTE.player_code = Info.player_code
                    JOIN
                    dbo.players AS P ON CTE.player_code = P.player_code
                    LEFT JOIN
                    dbo.player_ip_records AS PIR ON CTE.player_code = PIR.player_code
                        AND PIR.ip_type = 1",
            new
            {
                ReportDate = queryDate,
                UndepositCheckDate = queryDate.AddDays(-2)
            }).ConfigureAwait(false);

        foreach (var item in result)
            yield return item;
    }

    public async IAsyncEnumerable<PlayerDailyPaymentItem> QueryRealTimePlayerDailyDepositStatisticsAsync(
        Guid brandId,
        DateOnly queryDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (From, To) = (queryDate, queryDate.AddDays(1));
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

        var result = await conn.QueryAsync<RealTimePlayerPaymentItem>(
            @"
                WITH depositStatistics (
                    player_code,
                    deposit_amount,
                    daily_first_deposit_on,
                    daily_first_deposit_amount
                )
                AS
                (
                    SELECT
                        player_code,
                        Sum(deposit_amount) AS deposit_amount,
                        Min(daily_first_deposit_on) AS daily_first_deposit_on,
                        Min(daily_first_deposit_amount) AS daily_first_deposit_amount
                    FROM
                        (
                            SELECT
                                player_code,
                                COALESCE(actual_amount, amount) AS deposit_amount,
                                First_Value(updated_on) OVER w AS daily_first_deposit_on,
                                First_Value(COALESCE(actual_amount, amount)) OVER w AS daily_first_deposit_amount
                            FROM
                                dbo.deposit_forms
                            WHERE
                                brand_id = @BrandId
                                AND
                                status = 8000
                                AND
                                updated_on >= @Begin
                                AND
                                updated_on < @End
                            WINDOW w AS(PARTITION BY player_code ORDER BY updated_on ASC)
                        ) AS T
                    GROUP BY
                        player_code
                ),
                withdrawStatistics (
                    player_code,
                    withdraw_amount
                )
                AS
                (
                    SELECT
                        user_id,
                        Sum(COALESCE(actual_amount, amount)) AS withdraw_amount
                    FROM
                        dbo.withdraw_forms
                    WHERE
                        brand_id = @BrandId
                        AND
                        status = 9000
                        AND
                        user_type = 1
                        AND
                        paid_on >= @Begin
                        AND
                        paid_on < @End
                    GROUP BY
                        user_id
                ),
                mergeStatistics (
                    player_code,
                    daily_first_deposit_amount,
                    daily_first_deposit_on,
                    deposit_amount,
                    withdraw_amount,
                    is_current_deposit
                )
                AS
                (
                    SELECT
                        COALESCE(DS.player_code, WS.player_code) AS player_code,
                        DS.daily_first_deposit_amount,
                        DS.daily_first_deposit_on,
                        DS.deposit_amount,
                        WS.withdraw_amount,
                        CASE
                            WHEN DS.player_code IS NOT NULL THEN true
                            ELSE false
                        END AS is_current_deposit
                    FROM
                        depositStatistics AS DS
                        FULL OUTER JOIN
                        withdrawStatistics AS WS ON DS.player_code = WS.player_code
                ),
                undepositAgainStatistics (
                    player_code
                )
                AS
                (
                    SELECT
                        PS.player_code
                    FROM
                        dbo.player_daily_payment_statistics AS PS
                        JOIN
                        dbo.players AS P ON PS.player_code = P.player_code
                    WHERE
                        PS.report_date <= @UndepositAgainCheckDate
                        AND
                        P.brand_id = @BrandId
                    GROUP BY
                        PS.player_code
                    HAVING
                        Max(PS.total_deposit_count) = 1
                ),
                historyStatistics (
                    player_code,
                    first_deposit_on,
                    first_deposit_amount,
                    total_deposit_amount,
                    total_withdraw_amount
                )
                AS
                (
                    SELECT
                        player_code,
                        Min(first_deposit_on) AS first_deposit_on,
                        Min(first_deposit_amount) AS first_deposit_amount,
                        Max(total_deposit_amount) AS total_deposit_amount,
                        Max(total_withdraw_amount) AS total_withdraw_amount
                    FROM
                        dbo.player_daily_payment_statistics
                    WHERE
                        report_date < @QueryDate
                        AND
                        player_code IN(
                            SELECT
                                mergeStatistics.player_code
                            FROM
                                mergeStatistics
                            UNION
                            SELECT
                                undepositAgainStatistics.player_code
                            FROM
                                undepositAgainStatistics
                        )
                    GROUP BY
                        player_code
                )
                SELECT
                    COALESCE(MS.player_code, UAS.player_code) AS PlayerCode,
                    P.brand_id AS BrandId,
                    P.username AS UserName,
                    P.agent_id AS AgentId,
                    COALESCE(HS.first_deposit_on, MS.daily_first_deposit_on) AS FirstDepositOn,
                    COALESCE(HS.first_deposit_amount, MS.daily_first_deposit_amount) AS FirstDepositAmount,
                    COALESCE(MS.deposit_amount, 0) + COALESCE(HS.total_deposit_amount, 0) AS TotalDepositAmount,
                    COALESCE(MS.withdraw_amount, 0) + COALESCE(HS.total_withdraw_amount, 0) AS TotalWithdrawAmount,
                    CASE
                        WHEN UAS.player_code IS NULL THEN false
                        ELSE TRUE
                    END AS IsUndepositAgain,
                    MS.is_current_deposit AS IsCurrentDeposit,
                    PIR.ip AS RegisterIp
                FROM
                    mergeStatistics AS MS
                    FULL JOIN
                    undepositAgainStatistics AS UAS ON MS.player_code = UAS.player_code
                    LEFT JOIN
                    historyStatistics AS HS ON COALESCE(UAS.player_code, MS.player_code) = HS.player_code
                    JOIN
                    dbo.players AS P ON COALESCE(MS.player_code, UAS.player_code) = P.player_code
                        AND P.brand_id = @BrandId
                        AND P.agent_id IS NOT NULL
                    LEFT JOIN
                    dbo.player_ip_records AS PIR ON P.player_code = PIR.player_code
                        AND PIR.ip_type = 1",
            new
            {
                QueryDate = queryDate,
                Begin = queryTimeRange.From,
                End = queryTimeRange.To,
                UndepositAgainCheckDate = queryDate.AddDays(-2),
                BrandId = brandId
            }).ConfigureAwait(false);

        foreach (var item in result.Select(row => new PlayerDailyPaymentItem(
            row.PlayerCode,
            row.UserName,
            row.AgentId,
            row.IsCurrentDeposit == true
                ? row.FirstDepositOn >= queryTimeRange.From && row.FirstDepositOn < queryTimeRange.To
                : null,
            row.IsUndepositAgain,
            row.FirstDepositOn,
            row.FirstDepositAmount,
            row.TotalDepositAmount,
            row.TotalWithdrawAmount,
            row.RegisterIp)))
            yield return item;
    }
}

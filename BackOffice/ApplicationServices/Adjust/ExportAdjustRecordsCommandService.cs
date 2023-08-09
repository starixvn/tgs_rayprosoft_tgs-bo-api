#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LargeXlsx;
using Tgs.Agent.Client;
using Tgs.BrandCertificate.Clients;
using Tgs.CashQueryService;
using Tgs.CashQueryService.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;

internal class ExportAdjustRecordsCommandService
    : ICommandService<ExportAgentAdjustRecordsCommand>,
    ICommandService<ExportPlayerAdjustRecordsCommand>,
    ICommandService<ExportAllAdjustRecordsCommand>
{
    private readonly IAgentClient _agentClient;
    private readonly IQueryService<BrandRelationQuery, BrandRelationInfo> _brandRelationQueryService;
    private readonly ICashQueryClient _cashQueryClient;
    private readonly IPlayerServiceClient _playerServiceClient;

    public ExportAdjustRecordsCommandService(
        ICashQueryClient cashQueryClient,
        IQueryService<BrandRelationQuery, BrandRelationInfo> brandRelationQueryService,
        IAgentClient agentClient,
        IPlayerServiceClient playerServiceClient)
    {
        _cashQueryClient = cashQueryClient ?? throw new ArgumentNullException(nameof(cashQueryClient));
        _brandRelationQueryService = brandRelationQueryService ?? throw new ArgumentNullException(nameof(brandRelationQueryService));
        _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
    }

    public async ValueTask ExecuteAsync(ExportAgentAdjustRecordsCommand command, CancellationToken cancellationToken = default)
    {
        var brandCodeMap = await _brandRelationQueryService.QueryAsync(new(), cancellationToken)
            .ToDictionaryAsync(
                info => info.BrandId,
                info => info.BrandCode,
                cancellationToken)
            .ConfigureAwait(false);

        var brandCode = brandCodeMap[command.BrandId];

        var records = GetAgentAdjustRecordsAsync(
            command.BrandId,
            brandCode,
            command.CreatedOnRange,
            cancellationToken);

        using var xlsxWriter = new XlsxWriter(command.OutputStream);

        var headerStyle = new XlsxStyle(
            XlsxFont.Default,
            new XlsxFill(Color.AliceBlue),
            XlsxStyle.Default.Border,
            XlsxStyle.Default.NumberFormat,
            XlsxAlignment.Default);
        var dateStyle = XlsxStyle.Default.With(XlsxNumberFormat.ShortDateTime);

        _ = xlsxWriter
            .BeginWorksheet(brandCode)
            .SetDefaultStyle(headerStyle)
            .BeginRow()
            .Write("SN.")
            .Write("CreatedOn")
            .Write("Brand")
            .Write("Agent")
            .Write("UserType")
            .Write("AdjustType")
            .Write("Amount")
            .Write("Operator")
            .Write("Remark")
            .SetDefaultStyle(XlsxStyle.Default);

        var idx = 0;
        await foreach (var row in records
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            idx++;

            var displayAmount = row.OperationMode == OperationMode.Add.ToString()
                ? row.Amount
                : -row.Amount;

            _ = xlsxWriter
                .BeginRow()
                .Write(idx)
                .Write(row.CreatedOn, dateStyle)
                .Write(row.Brand)
                .Write(row.UserName)
                .Write(row.UserType)
                .Write(row.AdjustType)
                .Write(displayAmount)
                .Write(row.Operator)
                .Write(row.Remark);
        }
    }

    public async ValueTask ExecuteAsync(ExportPlayerAdjustRecordsCommand command, CancellationToken cancellationToken = default)
    {
        var brandCodeMap = await _brandRelationQueryService.QueryAsync(new(), cancellationToken)
            .ToDictionaryAsync(
                info => info.BrandId,
                info => info.BrandCode,
                cancellationToken)
            .ConfigureAwait(false);

        var brandCode = brandCodeMap[command.BrandId];

        var records = GetPlayerAdjustRecordsAsync(
            command.BrandId,
            brandCode,
            command.CreatedOnRange,
            cancellationToken);

        using var xlsxWriter = new XlsxWriter(command.OutputStream);

        var headerStyle = new XlsxStyle(
            XlsxFont.Default,
            new XlsxFill(Color.AliceBlue),
            XlsxStyle.Default.Border,
            XlsxStyle.Default.NumberFormat,
            XlsxAlignment.Default);
        var dateStyle = XlsxStyle.Default.With(XlsxNumberFormat.ShortDateTime);

        _ = xlsxWriter
            .BeginWorksheet(brandCode)
            .SetDefaultStyle(headerStyle)
            .BeginRow()
            .Write("SN.")
            .Write("CreatedOn")
            .Write("Brand")
            .Write("Player")
            .Write("UserType")
            .Write("AdjustType")
            .Write("Amount")
            .Write("Operator")
            .Write("Remark")
            .SetDefaultStyle(XlsxStyle.Default);

        var idx = 0;
        await foreach (var row in records
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            idx++;

            var displayAmount = row.OperationMode == OperationMode.Add.ToString()
                    ? row.Amount
                    : -row.Amount;

            _ = xlsxWriter
                .BeginRow()
                .Write(idx)
                .Write(row.CreatedOn, dateStyle)
                .Write(row.Brand)
                .Write(row.UserName)
                .Write(row.UserType)
                .Write(row.AdjustType)
                .Write(displayAmount)
                .Write(row.Operator)
                .Write(row.Remark);
        }
    }

    public async ValueTask ExecuteAsync(ExportAllAdjustRecordsCommand command, CancellationToken cancellationToken = default)
    {
        var brandCodeMap = await _brandRelationQueryService.QueryAsync(new(), cancellationToken)
            .ToDictionaryAsync(
                info => info.BrandId,
                info => info.BrandCode,
                cancellationToken)
            .ConfigureAwait(false);

        var records = command.BrandIds.ToAsyncEnumerable()
            .Select(brandId => (
                BrandCode: brandCodeMap[brandId],
                Items: GetPlayerAdjustRecordsAsync(
                    brandId,
                    brandCodeMap[brandId],
                    command.CreatedOnRange,
                    cancellationToken).Concat(GetAgentAdjustRecordsAsync(
                        brandId,
                        brandCodeMap[brandId],
                        command.CreatedOnRange,
                        cancellationToken))));

        using var xlsxWriter = new XlsxWriter(command.OutputStream);

        var headerStyle = new XlsxStyle(
            XlsxFont.Default,
            new XlsxFill(Color.AliceBlue),
            XlsxStyle.Default.Border,
            XlsxStyle.Default.NumberFormat,
            XlsxAlignment.Default);
        var dateStyle = XlsxStyle.Default.With(XlsxNumberFormat.ShortDateTime);

        await foreach (var (brandCode, items) in records
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            _ = xlsxWriter
                .BeginWorksheet(brandCode)
                .SetDefaultStyle(headerStyle)
                .BeginRow()
                .Write("SN.")
                .Write("CreatedOn")
                .Write("Brand")
                .Write("UserName")
                .Write("UserType")
                .Write("AdjustType")
                .Write("Amount")
                .Write("Operator")
                .Write("Remark")
                .SetDefaultStyle(XlsxStyle.Default);

            var idx = 0;
            await foreach (var row in items
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false))
            {
                idx++;

                var displayAmount = row.OperationMode == OperationMode.Add.ToString()
                    ? row.Amount
                    : -row.Amount;

                _ = xlsxWriter
                    .BeginRow()
                    .Write(idx)
                    .Write(row.CreatedOn, dateStyle)
                    .Write(row.Brand)
                    .Write(row.UserName)
                    .Write(row.UserType)
                    .Write(row.AdjustType)
                    .Write(displayAmount)
                    .Write(row.Operator)
                    .Write(row.Remark);
            }
        }
    }

    private IAsyncEnumerable<AdjustRow> GetAgentAdjustRecordsAsync(
        Guid brandId,
        string brandCode,
        (DateTime From, DateTime To) createdOnRange,
        CancellationToken cancellationToken = default)
        => AsyncEnumerableEx.Return(new Dictionary<string, string>())
            .SelectMany(map => AsyncEnumerableEx.Return(new AdjustRecordQueryCondition(
                createdOnRange,
                null,
                null,
                null))
                .SelectMany(condition => _cashQueryClient.CountAgentAdjustRecordsAsync(
                    brandId,
                    condition,
                    cancellationToken)
                    .AsTask()
                    .ToAsyncEnumerable()
                    .SelectMany(count => _cashQueryClient.QueryAgentAdjustRecordsAsync(
                        brandId,
                        condition,
                        count,
                        cancellationToken: cancellationToken)))
                .SelectMany(item => map.TryGetValue(item.UserId, out var agentName)
                    ? AsyncEnumerableEx.Return((Record: item, AgentName: agentName))
                    : _agentClient.GetAgentInfoAsync(
                        new(Guid.Parse(item.UserId)),
                        cancellationToken)
                        .AsTask()
                        .ToAsyncEnumerable()
                        .Do(agentInfo => map.Add(agentInfo!.Id.ToString(), agentInfo.Username))
                        .Select(agentInfo => (Record: item, AgentName: agentInfo!.Username))))
            .Select(tup => new AdjustRow(
                tup.Record.CreatedOn,
                brandCode,
                tup.AgentName,
                tup.Record.UserType.ToString(),
                tup.Record.AdjustType.ToString(),
                tup.Record.OperationMode.ToString(),
                tup.Record.Amount,
                tup.Record.Operator,
                tup.Record.Remark));

    private IAsyncEnumerable<AdjustRow> GetPlayerAdjustRecordsAsync(
        Guid brandId,
        string brandCode,
        (DateTime From, DateTime To) createdOnRange,
        CancellationToken cancellationToken = default)
        => AsyncEnumerableEx.Return(new Dictionary<string, string>())
            .SelectMany(map => AsyncEnumerableEx.Return(new AdjustRecordQueryCondition(
                createdOnRange,
                null,
                null,
                null))
                .SelectMany(condition => _cashQueryClient.CountPlayerAdjustRecordsAsync(
                    brandId,
                    condition,
                    cancellationToken)
                    .AsTask()
                    .ToAsyncEnumerable()
                    .SelectMany(count => _cashQueryClient.QueryPlayerAdjustRecordsAsync(
                        brandId,
                        new AdjustRecordQueryCondition(
                            createdOnRange,
                            null,
                            null,
                            null),
                        count,
                        cancellationToken: cancellationToken)))
                .Buffer(100)
                .SelectMany(items => _playerServiceClient
                    .QueryPlayerInfoListAsync(
                        brandId,
                        items.Select(item => item.UserId).Distinct()
                            .Where(playerCode => !map.ContainsKey(playerCode)),
                        cancellationToken)
                    .ForEachAsync(info => map.Add(info.PlayerCode, info.UserName), cancellationToken)
                    .ContinueWith(
                        t => map,
                        cancellationToken,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default)
                    .ToAsyncEnumerable()
                    .SelectMany(_ => items.ToAsyncEnumerable())
                    .Select(item => new AdjustRow(
                        item.CreatedOn,
                        brandCode,
                        map[item.UserId],
                        item.UserType.ToString(),
                        item.AdjustType.ToString(),
                        item.OperationMode.ToString(),
                        item.Amount,
                        item.Operator,
                        item.Remark))));

    private record AdjustRow(
        DateTime CreatedOn,
        string Brand,
        string UserName,
        string UserType,
        string AdjustType,
        string OperationMode,
        decimal Amount,
        string Operator,
        string? Remark);
}

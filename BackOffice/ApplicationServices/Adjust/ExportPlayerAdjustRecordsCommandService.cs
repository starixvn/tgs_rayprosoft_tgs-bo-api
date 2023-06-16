using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LargeXlsx;
using Tgs.BrandCertificate.Clients;
using Tgs.CashQueryService.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;

internal class ExportPlayerAdjustRecordsCommandService : ICommandService<ExportPlayerAdjustRecordsCommand>
{
    private readonly IQueryService<BrandRelationQuery, BrandRelationInfo> _brandRelationQueryService;
    private readonly ICashQueryClient _cashQueryClient;
    private readonly IPlayerServiceClient _playerServiceClient;

    public ExportPlayerAdjustRecordsCommandService(
        ICashQueryClient cashQueryClient,
        IQueryService<BrandRelationQuery, BrandRelationInfo> brandRelationQueryService,
        IPlayerServiceClient playerServiceClient)
    {
        _cashQueryClient = cashQueryClient ?? throw new ArgumentNullException(nameof(cashQueryClient));
        _brandRelationQueryService = brandRelationQueryService ?? throw new ArgumentNullException(nameof(brandRelationQueryService));
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
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

        var records = AsyncEnumerableEx.Return(new Dictionary<string, string>())
            .SelectMany(map => AsyncEnumerableEx.Return(new AdjustRecordQueryCondition(
                command.CreatedOnRange,
                null,
                null,
                null))
                .SelectMany(condition => _cashQueryClient.CountPlayerAdjustRecordsAsync(
                    command.BrandId,
                    condition,
                    cancellationToken)
                    .AsTask()
                    .ToAsyncEnumerable()
                    .SelectMany(count => _cashQueryClient.QueryPlayerAdjustRecordsAsync(
                        command.BrandId,
                        new AdjustRecordQueryCondition(
                            command.CreatedOnRange,
                            null,
                            null,
                            null),
                        count,
                        cancellationToken: cancellationToken)))
                .Select(item => (Record: item, BrandCode: brandCodeMap[item.BrandId]))
                .Buffer(100)
                .SelectMany(items => _playerServiceClient
                    .QueryPlayerInfoListAsync(
                        command.BrandId,
                        items.Select(item => item.Record.UserId).Distinct()
                            .Where(playerCode => map.ContainsKey(playerCode)),
                        cancellationToken)
                    .ForEachAsync(info => map.Add(info.PlayerCode, info.UserName), cancellationToken)
                    .ContinueWith(
                        t => map,
                        cancellationToken,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default)
                    .ToAsyncEnumerable()
                    .SelectMany(_ => items.ToAsyncEnumerable())
                    .Select(tup => new
                    {
                        tup.Record.CreatedOn,
                        Brand = tup.BrandCode,
                        UserName = map[tup.Record.UserId],
                        UserType = tup.Record.UserType.ToString(),
                        AdjustType = tup.Record.AdjustType.ToString(),
                        tup.Record.Amount,
                        tup.Record.Operator,
                        tup.Record.Remark
                    })));

        using var xlsxWriter = new XlsxWriter(command.OutputStream);

        var headerStyle = new XlsxStyle(
            XlsxFont.Default,
            new XlsxFill(Color.AliceBlue),
            XlsxStyle.Default.Border,
            XlsxStyle.Default.NumberFormat,
            XlsxAlignment.Default);

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
        await foreach (var row in records.ConfigureAwait(false))
        {
            idx++;
            _ = xlsxWriter
                .BeginRow()
                .Write(idx)
                .Write(row.CreatedOn)
                .Write(row.Brand)
                .Write(row.UserName)
                .Write(row.UserType)
                .Write(row.AdjustType)
                .Write(row.Amount)
                .Write(row.Operator)
                .Write(row.Remark);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LargeXlsx;
using Tgs.Agent.Client;
using Tgs.BrandCertificate.Clients;
using Tgs.CashQueryService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;

internal class ExportAgentAdjustRecordsCommandService : ICommandService<ExportAgentAdjustRecordsCommand>
{
    private readonly IAgentClient _agentClient;
    private readonly IQueryService<BrandRelationQuery, BrandRelationInfo> _brandRelationQueryService;
    private readonly ICashQueryClient _cashQueryClient;

    public ExportAgentAdjustRecordsCommandService(
        ICashQueryClient cashQueryClient,
        IQueryService<BrandRelationQuery, BrandRelationInfo> brandRelationQueryService,
        IAgentClient agentClient)
    {
        _cashQueryClient = cashQueryClient ?? throw new ArgumentNullException(nameof(cashQueryClient));
        _brandRelationQueryService = brandRelationQueryService ?? throw new ArgumentNullException(nameof(brandRelationQueryService));
        _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
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

        var records = AsyncEnumerableEx.Return(new Dictionary<string, string>())
            .SelectMany(map => AsyncEnumerableEx.Return(new AdjustRecordQueryCondition(
                command.CreatedOnRange,
                null,
                null,
                null))
                .SelectMany(condition => _cashQueryClient.CountAgentAdjustRecordsAsync(
                    command.BrandId,
                    condition,
                    cancellationToken)
                    .AsTask()
                    .ToAsyncEnumerable()
                    .SelectMany(count => _cashQueryClient.QueryAgentAdjustRecordsAsync(
                        command.BrandId,
                        condition,
                        count,
                        cancellationToken: cancellationToken)))
                .Select(item => (Record: item, BrandCode: brandCodeMap[item.BrandId]))
                .SelectMany(tup => map.TryGetValue(tup.Record.UserId, out var agentName)
                    ? AsyncEnumerableEx.Return((tup.Record, tup.BrandCode, AgentName: agentName))
                    : _agentClient.GetAgentInfoAsync(
                        new(Guid.Parse(tup.Record.UserId)),
                        cancellationToken)
                        .AsTask()
                        .ToAsyncEnumerable()
                        .Do(agentInfo => map.Add(agentInfo.Id.ToString(), agentInfo.Username))
                        .Select(agentInfo => (tup.Record, tup.BrandCode, AgentName: agentInfo.Username))))
            .Select(tup => new
            {
                tup.Record.CreatedOn,
                Brand = tup.BrandCode,
                UserName = tup.AgentName,
                UserType = tup.Record.UserType.ToString(),
                AdjustType = tup.Record.AdjustType.ToString(),
                tup.Record.Amount,
                tup.Record.Operator,
                tup.Record.Remark
            });

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

using NSubstitute;
using Tgs.CashQueryService.Client;
using TGS.Services.BackOffice.ApplicationServices.Adjust;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AgentAdjustRecordsQueryServiceTests
{
    [Test]
    public async Task 取得代理調帳記錄()
    {
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();

        var sut = new AgentAdjustRecordsQueryService(fakeCashQueryClient);

        var query = new AgentAdjustRecordsQuery(
            Guid.NewGuid(),
            (DateTime.Now.AddDays(-1D), DateTime.Now),
            null,
            null,
            null);

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeCashQueryClient.Received(1)
            .QueryAgentAdjustRecordsAsync(
                Arg.Is(query.BrandId),
                Arg.Is(new AdjustRecordQueryCondition(
                    query.CreatedOnRange,
                    query.SearchTypes,
                    query.AgentId?.ToString(),
                    query.Operator)),
                Arg.Is(query.Skip));
    }

    [Test]
    public async Task 取得代理調帳記錄筆數()
    {
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();

        var sut = new AgentAdjustRecordsQueryService(fakeCashQueryClient);

        var query = new AgentAdjustRecordsQuery(
            Guid.NewGuid(),
            (DateTime.Now.AddDays(-1D), DateTime.Now),
            null,
            null,
            null);

        _ = await sut.GetAsync(query);

        _ = fakeCashQueryClient.Received(1)
            .CountAgentAdjustRecordsAsync(Arg.Is(query.BrandId),
                Arg.Is(new AdjustRecordQueryCondition(
                    query.CreatedOnRange,
                    query.SearchTypes,
                    query.AgentId?.ToString(),
                    query.Operator)));
    }
}

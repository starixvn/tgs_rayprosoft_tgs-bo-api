using Tgs.CashQueryService.Client;
using TGS.Services.BackOffice.ApplicationServices.Adjust;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AdjustSummariesQueryServiceTests
{
    [Test]
    public async Task 取得調帳統計資訊()
    {
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();

        var sut = new AdjustSummariesQueryService(fakeCashQueryClient);

        var query = new AdjustSummariesQuery(
            (DateTime.Now.AddDays(-1D), DateTime.Now));

        var expectedCondition = new AdjustRecordQueryCondition(
            query.CreatedOnRange,
            null,
            null,
            null);

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeCashQueryClient.Received(1)
            .QueryPlayerAdjustSummariesAsync(Arg.Is(expectedCondition));
        _ = fakeCashQueryClient.Received(1)
            .QueryAgentAdjustSummariesAsync(Arg.Is(expectedCondition));
    }
}

using NSubstitute;
using Tgs.CashQueryService.Client;
using TGS.Services.BackOffice.ApplicationServices.Adjust;

namespace TGS.Tests.BackendOffice.UnitTests;

public class PlayerAdjustRecordsQueryServiceTests
{
    [Test]
    public async Task 取得玩家調帳記錄()
    {
        var fakeCashQueryServiceClient = Substitute.For<ICashQueryClient>();

        var sut = new PlayerAdjustRecordsQueryService(fakeCashQueryServiceClient);

        var query = new PlayerAdjustRecordsQuery(
            Guid.NewGuid(),
            (DateTime.Now.AddDays(-1D), DateTime.Now),
            null,
            null,
            null);

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeCashQueryServiceClient.Received(1)
            .QueryPlayerAdjustRecordsAsync(
                Arg.Is(query.BrandId),
                Arg.Is(new AdjustRecordQueryCondition(
                    query.CreatedOnRange,
                    query.SearchTypes,
                    query.PlayerCode,
                    query.Operator)),
                Arg.Is(query.Skip));
    }

    [Test]
    public async Task 取得玩家調帳記錄筆數()
    {
        var fakeCashQueryServiceClient = Substitute.For<ICashQueryClient>();

        var sut = new PlayerAdjustRecordsQueryService(fakeCashQueryServiceClient);

        var query = new PlayerAdjustRecordsQuery(
            Guid.NewGuid(),
            (DateTime.Now.AddDays(-1D), DateTime.Now),
            null,
            null,
            null);

        _ = await sut.GetAsync(query);

        _ = fakeCashQueryServiceClient.Received(1)
            .CountPlayerAdjustRecordsAsync(
                Arg.Is(query.BrandId),
                Arg.Is(new AdjustRecordQueryCondition(
                    query.CreatedOnRange,
                    query.SearchTypes,
                    query.PlayerCode,
                    query.Operator)));
    }
}

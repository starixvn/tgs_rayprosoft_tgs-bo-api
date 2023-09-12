using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.ApplicationServices.Overview;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class OverviewTests
{
    [Test]
    public async Task 查詢存提差()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new GetDayNetDepositQueryService(fakePaymentServiceClient);

        var brandId = Guid.NewGuid();
        var yesterday = DateOnly.FromDateTime(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8d)).AddDays(-1).DateTime);

        _ = fakePaymentServiceClient
            .GetBrandCurrentDayNetDepositAsync(Arg.Is(brandId))
            .Returns(168888m);

        _ = fakePaymentServiceClient
            .GetBrandReportDateNetDepositAsync(Arg.Is(brandId), Arg.Is(yesterday))
            .Returns(16888m);

        var actual = await sut.GetAsync(new GetDayNetDepositQuery(brandId));

        Assert.AreEqual(168888m, actual.Today);
        Assert.AreEqual(16888m, actual.Yesterday);
    }

    [Test]
    public async Task 查詢存款成功率()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new GetDaySuccessRateQueryService(fakePaymentServiceClient);

        var brandId = Guid.NewGuid();
        var yesterday = DateOnly.FromDateTime(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8d)).AddDays(-1).DateTime);

        _ = fakePaymentServiceClient
            .GetCurrentDayDepositSuccessRateAsync(Arg.Is(brandId))
            .Returns(100m);

        _ = fakePaymentServiceClient
            .GetBrandSuccessRateByReportDateAsync(Arg.Is(brandId), Arg.Is(yesterday))
            .Returns(88m);

        var actual = await sut.GetAsync(new GetDaySuccessRateQuery(brandId));

        Assert.AreEqual(100m, actual.Today);
        Assert.AreEqual(88m, actual.Yesterday);
    }

    [Test]
    public async Task 查詢當日與昨天註冊人數()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new GetDayPlayerRegisterCountQueryService(fakePlayerServiceClient);

        var brandId = Guid.NewGuid();
        var yesterday = DateOnly.FromDateTime(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8d)).AddDays(-1).DateTime);

        _ = fakePlayerServiceClient
            .CountCurrentDayRegisterPlayersAsync(Arg.Is(brandId))
            .Returns(168);

        _ = fakePlayerServiceClient
            .CountReportDateRegisterPlayersAsync(Arg.Is(brandId), Arg.Is(yesterday))
            .Returns(88);

        var actual = await sut.GetAsync(new GetDayPlayerRegisterCountQuery(brandId));

        Assert.AreEqual(168, actual.Today);
        Assert.AreEqual(88, actual.Yesterday);
    }
}

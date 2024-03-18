using Tgs.Report.Client;
using AutoFixture;
using Tgs.Report.Client.Models;
using TGS.Services.BackOffice.Application.Queries.ReportBrandGameProviderQueries;
using Tgs.Report.Client.Models.BrandGameProvider;
using TGS.Services.BackOffice.Application.Models.ReportBrandGameProviderRound;

namespace TGS.Tests.BackendOffice.UnitTests.Report;

public class BrandGameProviderReportTests
{
    [Test]
    public async Task Can_Get_Brand_Game_Providers()
    {
        var fakeReportClient = Substitute.For<IReportClient>();

        var sut = new GetBrandGameProvidersQuery(fakeReportClient);

        var brandId = Guid.NewGuid();

        var request = new GetBrandGameProvidersReportRequest() { BrandId = brandId };

        var fixture = new Fixture();

        var brandGameProviders = fixture.CreateMany<BrandGameProvidersModel>(10).ToArray();

        _ = fakeReportClient.GetBrandGameProvidersAsync(
                Arg.Any<GetBrandGameProvidersRequest>())
            .Returns(new GetBrandGameProvidersResponse
            {
                Datas = new PagedResponse<BrandGameProvidersModel>
                {
                    Items = brandGameProviders
                },
                Total = brandGameProviders.GroupBy(x => x).Select(x => new BrandGameProviderTotalModel
                {
                    TransactionCount = x.Sum(s => s.TransactionCount),
                    PlayerCount = x.Sum(s => s.PlayerCount)
                }).First()
            });

        var response = await sut.GetAsync(request, default);

        Assert.That(response.Datas.Items, Has.Length.EqualTo(10));

        _ = fakeReportClient.Received(1)
            .GetBrandGameProvidersAsync(Arg.Any<GetBrandGameProvidersRequest>());
    }
}
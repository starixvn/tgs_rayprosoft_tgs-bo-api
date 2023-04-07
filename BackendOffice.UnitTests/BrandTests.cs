using NSubstitute;
using Tgs.BrandCertificate.Clients;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;
using BrandListQuery = TGS.Services.BackOffice.ApplicationServices.BrandListQuery;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
internal class BrandTests
{
    [Test]
    public async Task 取得BrandInfo()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new GetBrandInfoQueryService(fakeBrandServiceClient);

        var query = new GetBrandInfoQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = fakeBrandServiceClient.Received(1)
            .GetBrandInfoAsync(
                Arg.Is(query.BrandId),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 取得Brand列表()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new BrandListQueryService(fakeBrandServiceClient);

        var query = new BrandListQuery(
            Array.Empty<Guid>(),
            null,
            null);

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeBrandServiceClient.Received(1)
            .QueryBrandListAsync(
                Arg.Is<Tgs.BrandCertificate.Clients.BrandListQuery>(x =>
                    x.BrandIds.SequenceEqual(query.BrandIs)),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 取得Brand關聯表()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new BrandRelationQueryService(fakeBrandServiceClient);

        var query = new BrandRelationQuery();

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeBrandServiceClient.Received(1)
            .GetBrandRelationsAsync(
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 清除Brand關聯表快取()
    {
        var fakeClearCacheService = Substitute.For<IClearBrandRelationCacheService>();

        var sut = new ClearBrandRelationCacheCommandService(fakeClearCacheService);

        var command = new ClearBrandRelationCacheCommand();

        await sut.ExecuteAsync(command);

        _ = fakeClearCacheService.Received(1)
            .ClearAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 建立Brand()
    {
        var fakeBrandService = Substitute.For<IBrandService>();

        var sut = new CreateBrandCommandService(fakeBrandService);

        var languages = new[] { 
            new BrandLanguageModel
            {
                BrandCultureCode = "en-US",
                CultureCode = "en-US"
            } };

        var gameProviders = new[]
        {
            new BrandGameProviderModel
            {
                GameProviderId = Guid.NewGuid(),
                ConfigurationId = Guid.NewGuid(),
                DefaultLanguageCode = "en-US",
                Name = "PPTF"
            }
        };
        
        var command = new CreateBrandCommand(
            "Brand1",
            "TestBrand",
            "TestBrand",
            string.Empty,
            10000m,
            480,
            false,
            Guid.NewGuid(),
            new [] {"CNY"},
            languages,
            SettlementIntervalType.Daily,
            7,
            gameProviders,
            AgentDividendType.OneLayer,
            false,
            false,
            0m,
            100m,
            100m,
            Array.Empty<string>(),
            Array.Empty<BrandBetLimitModel>(),
            "User1");

        _ = await sut.GetAsync(command);

        _ = fakeBrandService.Received(1)
            .CreateBrandAsync(
                Arg.Is<CreateBrandInfo>(x => 
                    x.Code == command.Code 
                    && x.LicenseeId == command.LicenseeId),
                Arg.Any<CancellationToken>());
    }
}

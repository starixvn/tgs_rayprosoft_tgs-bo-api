using System.Text.Json;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.BackOffice.Infrastructure.RedisServices;
using TGS.SmsService.Client;
using BrandListQuery = TGS.Services.BackOffice.ApplicationServices.BrandListQuery;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
internal class BrandTests
{
    [Test]
    public async Task 更新BrandStatus()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeStatusChangedCommandService = Substitute.For<ICommandService<HandleBrandStatusChangedCommand>>();

        var sut = new UpdateBrandStatusCommandService(
            fakeBrandServiceClient,
            fakeStatusChangedCommandService);

        var command = new UpdateBrandStatusCommand(
            Guid.NewGuid(),
            BrandStatus.Inactive,
            "User1");

        await sut.ExecuteAsync(command);

        _ = fakeBrandServiceClient.Received(1)
            .UpdateBrandStatusAsync(
                Arg.Is(command.BrandId),
                Arg.Is(Tgs.BrandCertificate.BrandStatus.Inactive),
                Arg.Is(command.Modifier),
                Arg.Any<CancellationToken>());

        _ = fakeStatusChangedCommandService.Received(1)
            .ExecuteAsync(
                Arg.Is(new HandleBrandStatusChangedCommand(command.BrandId, command.Status)),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 更新BrandStatus_會清除Brand的RedisCache()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeRedisCacheManager = Substitute.For<IRedisCacheManager>();
        var fakeMaintenanceRedisService = Substitute.For<IMaintenanceRedisService>();

        var sut = new HandleBrandStatusChangedCommandService(
            fakeBrandServiceClient,
            fakeRedisCacheManager,
            fakeMaintenanceRedisService);

        var command = new HandleBrandStatusChangedCommand(
            Guid.NewGuid(),
            BrandStatus.Inactive);

        await sut.ExecuteAsync(command);

        _ = fakeRedisCacheManager.Received(1)
            .ClearCacheAsync(
                Arg.Is(RedisCacheType.Brand),
                Arg.Is(command.BrandId));
    }

    [Test]
    public async Task 更新BrandStatus為Active_會清除Brand維護的RedisCache()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeRedisCacheManager = Substitute.For<IRedisCacheManager>();
        var fakeMaintenanceRedisService = Substitute.For<IMaintenanceRedisService>();

        var sut = new HandleBrandStatusChangedCommandService(
            fakeBrandServiceClient,
            fakeRedisCacheManager,
            fakeMaintenanceRedisService);

        var command = new HandleBrandStatusChangedCommand(
            Guid.NewGuid(),
            BrandStatus.Active);

        _ = fakeBrandServiceClient.GetBrandStatusAsync(
                Arg.Is(command.BrandId),
                Arg.Any<CancellationToken>())
            .Returns(Tgs.BrandCertificate.BrandStatus.Active);

        await sut.ExecuteAsync(command);

        _ = fakeMaintenanceRedisService.Received(1)
            .ClearBrandMaintenanceAsync(
                Arg.Is(command.BrandId));
    }

    [Test]
    public async Task 更新BrandStatus為Maintenance_會新增Brand維護的RedisCache()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeRedisCacheManager = Substitute.For<IRedisCacheManager>();
        var fakeMaintenanceRedisService = Substitute.For<IMaintenanceRedisService>();

        var sut = new HandleBrandStatusChangedCommandService(
            fakeBrandServiceClient,
            fakeRedisCacheManager,
            fakeMaintenanceRedisService);

        var command = new HandleBrandStatusChangedCommand(
            Guid.NewGuid(),
            BrandStatus.Maintenance);

        _ = fakeBrandServiceClient.GetBrandStatusAsync(
                Arg.Is(command.BrandId),
                Arg.Any<CancellationToken>())
            .Returns(Tgs.BrandCertificate.BrandStatus.Maintenance);

        await sut.ExecuteAsync(command);

        _ = fakeMaintenanceRedisService.Received(1)
            .AddBrandMaintenanceAsync(
                Arg.Is(command.BrandId));
    }

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
            [],
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
    public async Task 取得全部Brand的幣別()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new AllBrandCurrenciesQueryService(fakeBrandServiceClient);

        var query = new AllBrandCurrenciesQuery();

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeBrandServiceClient.Received(1)
            .GetAllBrandCurrenciesAsync();
    }

    [Test]
    public async Task 建立Brand()
    {
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new CreateBrandCommandService(fakeBrandService, fakeBrandServiceClient);

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
            480,
            false,
            Guid.NewGuid(),
            ["CNY"],
            languages,
            SettlementIntervalType.Daily,
            7,
            gameProviders,
            AgentDividendType.OneLayer,
            false,
            false,
            true,
            true,
            0m,
            100m,
            100m,
            [],
            [],
            "User1");

        _ = await sut.GetAsync(command);

        _ = fakeBrandService.Received(1)
            .CreateBrandAsync(
                Arg.Any<CreateBrandInfo>(),
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
    public async Task SetBrandEnableSmsCommandService_開關平台簡訊設定()
    {
        // Arrange
        var fakeClient = Substitute.For<IBrandServiceClient>();
        var sut = new SetBrandEnableSmsCommandService(fakeClient);

        var brandId = Guid.NewGuid();
        var enableSms = true;
        var modifier = "test";
        var command = new SetBrandEnableSmsCommand(brandId, enableSms, modifier);
        // Act
        await sut.ExecuteAsync(command);
        // Assert
        _ = fakeClient.Received(1)
            .SetBrandEnableSmsAsync(
                Arg.Is(brandId),
                Arg.Is(enableSms),
                Arg.Is(modifier));
    }

    [Test]
    public async Task ApplyBrandSmsCommandService_套用平台簡訊設定()
    {
        // Arrange
        var fakeClient = Substitute.For<ISmsServiceClient>();
        var sut = new ApplyBrandSmsCommandService(fakeClient);

        var brandId = Guid.NewGuid();
        var smsSettingId = Guid.NewGuid();
        var modifier = "test";
        var command = new ApplyBrandSmsCommand(brandId, smsSettingId, modifier);

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeClient.Received(1)
            .ApplyBrandSmsAsync(
                Arg.Is(brandId),
                Arg.Is(smsSettingId),
                Arg.Is(modifier));
    }

    [Test]
    public async Task GetBrandSmsQueryService_取得平台使用的簡訊設定()
    {
        // Arrange
        var fakeClient = Substitute.For<IBrandServiceClient>();
        var sut = new GetBrandSmsQueryService(fakeClient);

        var brandId = Guid.NewGuid();
        var query = new GetBrandSmsQuery(brandId);

        _ = fakeClient
            .GetBrandInfoAsync(Arg.Is(brandId))
            .Returns(new BrandInfo()
            {
                Id = brandId,
                SmsEnabled = true
            });

        _ = fakeClient
            .GetBrandSmsSettingsAsync<JsonDocument>(Arg.Is(brandId))
            .Returns(new SmsVendorInfo<JsonDocument>("Mock", JsonDocument.Parse("{}")));
        // Act
        var actual = await sut.GetAsync(query);
        // Assert

        Assert.That(actual.EnableSms, Is.EqualTo(true));
        Assert.That(actual.SmsVendorCode, Is.EqualTo("Mock"));
    }
}
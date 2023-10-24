using System.Runtime.InteropServices;
using ExpectedObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.VipLevelService.Clients;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class VipLevelTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly NodeJsFixture _nodeJsFixture;

    public VipLevelTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));
    }

    [Fact]
    public async Task 更新VipLevel設定()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
            .ConfigureServices(services => services
                .AddAuthentication("Fake")
                .AddScheme<FakeAuthenticationSchemaOptions, FakeAuthenticationHandler>(
                    "Fake",
                    options =>
                    {
                        options.UserId = userId;
                        options.UserName = "system";
                    })
                .Services
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeVipLevelServiceClient))
            .Build();

        await web.StartAsync();

        var vipLevelId = Guid.NewGuid();

        var updateVipLevelModel = new ModifyVipLevelViewModel
        {
            BrandId = Guid.NewGuid(),
            BirthBenefit = 1M,
            CardGameRebate = 2M,
            Level = 3,
            LevelKeepAccumulateBetAmount = 4M,
            LevelKeepAccumulateDeposit = 5M,
            LevelKeepCondition = true,
            LevelUpAccumulateBetAmount = 6M,
            LevelUpAccumulateDeposit = 7M,
            LiveDealerRebate = 8M,
            LotteryRebate = 9M,
            PromotionBenefit = 10M,
            QuarterlyBenefit = 11M,
            SlotRebate = 12M,
            SportsRebate = 13M,
            VirtualSportsRebate = 14M,
            WithdrawTotalAmountPerDay = 15M,
            WithdrawTransactionsPerDay = 16
        };

        _ = fakeVipLevelServiceClient.GetBrandAllVipLevelsAsync(
            Arg.Is(updateVipLevelModel.BrandId),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new Tgs.VipLevelService.Clients.VipLevelInfo(
                    vipLevelId,
                    2,
                    updateVipLevelModel.BrandId,
                    11M,
                    12M,
                    13M,
                    14M,
                    true,
                    16M,
                    17M,
                    18M,
                    19M,
                    20M,
                    21M,
                    22M,
                    23M,
                    24M,
                    25,
                    26)
            }.ToAsyncEnumerable());

        var expected = new VipLevelUpdateInfo(
            updateVipLevelModel.Level,
            updateVipLevelModel.LevelUpAccumulateBetAmount,
            updateVipLevelModel.LevelUpAccumulateDeposit,
            updateVipLevelModel.LevelKeepAccumulateBetAmount,
            updateVipLevelModel.LevelKeepAccumulateDeposit,
            updateVipLevelModel.LevelKeepCondition,
            updateVipLevelModel.BirthBenefit,
            updateVipLevelModel.PromotionBenefit,
            updateVipLevelModel.QuarterlyBenefit,
            updateVipLevelModel.LiveDealerRebate,
            updateVipLevelModel.SlotRebate,
            updateVipLevelModel.LotteryRebate,
            updateVipLevelModel.CardGameRebate,
            updateVipLevelModel.SportsRebate,
            updateVipLevelModel.VirtualSportsRebate,
            updateVipLevelModel.WithdrawTransactionsPerDay,
            updateVipLevelModel.WithdrawTotalAmountPerDay);

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, id, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.updateVipLevel(brandId, id, request);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { updateVipLevelModel.BrandId, vipLevelId, updateVipLevelModel });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual!.Valid);
        _ = fakeVipLevelServiceClient.Received(1)
            .UpdateVipLevelAsync(
                Arg.Is(vipLevelId),
                Arg.Is(expected),
                Arg.Is("system"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 取得Brand的VipLevel設定列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
            .ConfigureServices(services => services
                .AddAuthentication("Fake")
                .AddScheme<FakeAuthenticationSchemaOptions, FakeAuthenticationHandler>(
                    "Fake",
                    options =>
                    {
                        options.UserId = userId;
                        options.UserName = "system";
                    })
                .Services
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeVipLevelServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        var expected = new[]
        {
            new Tgs.VipLevelService.Clients.VipLevelInfo(
                Guid.NewGuid(),
                1,
                brandId,
                1M,
                2M,
                3M,
                4M,
                true,
                6M,
                7M,
                8M,
                9M,
                10M,
                11M,
                12M,
                13M,
                14M,
                15,
                16),
            new Tgs.VipLevelService.Clients.VipLevelInfo(
                Guid.NewGuid(),
                2,
                brandId,
                11M,
                12M,
                13M,
                14M,
                true,
                16M,
                17M,
                18M,
                19M,
                20M,
                21M,
                22M,
                23M,
                24M,
                25,
                26)
        };

        _ = fakeVipLevelServiceClient.GetBrandAllVipLevelsAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(expected.ToAsyncEnumerable());

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getVipLevels(brandId);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<PagedResponse<VipLevelViewModel>>(
            nodeModule,
            args: new object[] { brandId });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.Valid);
        expected.ToExpectedObject(ctx => ctx
            .IgnoreEqualsOverride()).ShouldMatch(actual.Items);
    }

    [Fact]
    public async Task 建立VipLevel設定()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
            .ConfigureServices(services => services
                .AddAuthentication("Fake")
                .AddScheme<FakeAuthenticationSchemaOptions, FakeAuthenticationHandler>(
                    "Fake",
                    options =>
                    {
                        options.UserId = userId;
                        options.UserName = "system";
                    })
                .Services
                .AddSingleton(fakeBrandServiceClient)
                .AddSingleton(fakeVipLevelServiceClient))
            .Build();

        await web.StartAsync();

        var createVipLevelViewModel = new ModifyVipLevelViewModel
        {
            BrandId = Guid.NewGuid(),
            BirthBenefit = 1M,
            CardGameRebate = 2M,
            Level = 3,
            LevelKeepAccumulateBetAmount = 4M,
            LevelKeepAccumulateDeposit = 5M,
            LevelKeepCondition = true,
            LevelUpAccumulateBetAmount = 6M,
            LevelUpAccumulateDeposit = 7M,
            LiveDealerRebate = 8M,
            LotteryRebate = 9M,
            PromotionBenefit = 10M,
            QuarterlyBenefit = 11M,
            SlotRebate = 12M,
            SportsRebate = 13M,
            VirtualSportsRebate = 14M,
            WithdrawTotalAmountPerDay = 15M,
            WithdrawTransactionsPerDay = 16
        };

        _ = fakeVipLevelServiceClient.GetBrandAllVipLevelsAsync(
            Arg.Is(createVipLevelViewModel.BrandId),
            Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<Tgs.VipLevelService.Clients.VipLevelInfo>());

        var expected = new VipLevelCreateInfo(
            createVipLevelViewModel.LevelUpAccumulateBetAmount,
            createVipLevelViewModel.LevelUpAccumulateDeposit,
            createVipLevelViewModel.LevelKeepAccumulateBetAmount,
            createVipLevelViewModel.LevelKeepAccumulateDeposit,
            createVipLevelViewModel.LevelKeepCondition,
            createVipLevelViewModel.BirthBenefit,
            createVipLevelViewModel.PromotionBenefit,
            createVipLevelViewModel.QuarterlyBenefit,
            createVipLevelViewModel.LiveDealerRebate,
            createVipLevelViewModel.SlotRebate,
            createVipLevelViewModel.LotteryRebate,
            createVipLevelViewModel.CardGameRebate,
            createVipLevelViewModel.SportsRebate,
            createVipLevelViewModel.VirtualSportsRebate,
            createVipLevelViewModel.WithdrawTransactionsPerDay,
            createVipLevelViewModel.WithdrawTotalAmountPerDay);

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.createVipLevel(brandId, request);
            };
        """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] { createVipLevelViewModel.BrandId, createVipLevelViewModel });

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual!.Valid);
        _ = fakeVipLevelServiceClient.Received(1)
            .CreateVipLevelAsync(
                Arg.Is(createVipLevelViewModel.BrandId),
                Arg.Is(expected),
                Arg.Is("system"),
                Arg.Any<CancellationToken>());
    }
}

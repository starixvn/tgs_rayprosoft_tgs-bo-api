using System.Runtime.InteropServices;
using ExpectedObjects;
using Google.Protobuf;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.CashService.Client;
using Tgs.PlayerBucket.Client;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.Services.PlayerBucket.Protos;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.MessageQueue;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;
using TGS.Services.BackOffice.ApplicationServices.Players;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class PlayerBucketTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PlayerBucketTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 取得玩家概括流水量()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

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
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePlayerBucketClient)
                .AddSingleton(fakePlayerBucketForTypesClient))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var brandId = Guid.NewGuid();

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode,
                BrandId = brandId
            });

        _ = fakePlayerBucketForTypesClient.ReadBucketsForBucketTypesAsync(
            Arg.Is(playerCode),
            Arg.Is<IEnumerable<string>>(types => types.Count() == 0),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new TypeBucketResult(
                    null,
                    new ReadBucketResult(
                        1000M,
                        Guid.NewGuid()))
            }.ToAsyncEnumerable());

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "true"
                        : "false")}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerBucketSummaryAmount(playerCode);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<decimal>(
            nodeModule,
            args: new object[] { playerCode });

        // Assert
        Assert.Equal(1000M, actual);
    }

    [Fact]
    public async Task 取得玩家概括流水量_多個值將會以最大的值回傳()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

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
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePlayerBucketClient)
                .AddSingleton(fakePlayerBucketForTypesClient))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var brandId = Guid.NewGuid();

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode,
                BrandId = brandId
            });

        _ = fakePlayerBucketClient.GetBrandBucketTypesAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                "Game1",
                "Game2"
            }.ToAsyncEnumerable());

        _ = fakePlayerBucketForTypesClient.ReadBucketsForBucketTypesAsync(
            Arg.Is(playerCode),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new TypeBucketResult(
                    null,
                    new ReadBucketResult(
                        1000M,
                        Guid.NewGuid())),
                new TypeBucketResult(
                    "Game1",
                    new ReadBucketResult(
                        5000M,
                        Guid.NewGuid())),
                new TypeBucketResult(
                    "Game2",
                    new ReadBucketResult(
                        0M,
                        Guid.NewGuid())),
            }.ToAsyncEnumerable());

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "true"
                        : "false")}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerBucketSummaryAmount(playerCode);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<decimal>(
            nodeModule,
            args: new object[] { playerCode });

        // Assert
        Assert.Equal(5000M, actual);
    }

    [Fact]
    public async Task 查詢清除流水表單()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

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
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePlayerBucketClient))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var forms = new[]
        {
            new CleanBucketForm(
                Guid.NewGuid(),
                playerCode,
                Guid.NewGuid(),
                Guid.NewGuid(),
                123M,
                new[]
                {
                    new TypeBucketResult(
                        null,
                        new ReadBucketResult(10000M, Guid.NewGuid()))
                },
                Guid.NewGuid(),
                100M,
                124M,
                new[]
                {
                    new TypeBucketResult(
                        null,
                        new ReadBucketResult(11000M, Guid.NewGuid()))
                },
                CleanFlowStatus.Completed,
                DateTime.UtcNow),
            new CleanBucketForm(
                Guid.NewGuid(),
                playerCode,
                Guid.NewGuid(),
                Guid.NewGuid(),
                456M,
                new[]
                {
                    new TypeBucketResult(
                        null,
                        new ReadBucketResult(20000M, Guid.NewGuid()))
                },
                Guid.NewGuid(),
                100M,
                null,
                null,
                CleanFlowStatus.Create,
                DateTime.UtcNow.AddMinutes(1D))
        };

        var expected = forms
            .Select(form => new CleanBucketFormViewModel
            {
                Id = form.Id,
                CleaningBalance = form.CleaningBalance,
                CleaningBucketAmount = form.CleaningBucketAmounts
                    ?.FirstOrDefault()
                    ?.Result
                    .Amount,
                CreatedOn = form.CreatedOn,
                DepositFormId = form.DepositFormId,
                PlayerCode = form.PlayerCode,
                ReviewBalance = form.CurrentBalance,
                ReviewBucketAmount = form.CurrentBucketAmounts
                    .First()
                    .Result
                    .Amount,
                Status = form.Status.ToString()
            })
            .ToArray();

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "true"
                        : "false")}});

                let svc = new BackOfficeService(instance);

                return await svc.queryCleanBucketForms(playerCode);
            };
        """;

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo());

        _ = fakePlayerBucketClient.QueryCleanBucketFomsAsync(
            Arg.Is(playerCode),
            Arg.Is(default(CleanFlowStatus?)),
            Arg.Any<CancellationToken>())
            .Returns(forms.ToAsyncEnumerable());

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<CleanBucketFormViewModel[]>(
            nodeModule,
            args: new object[] { playerCode });

        // Assert
        expected
            .ToExpectedObject(ctx => ctx
                .ArrayMemberShouldMatch(o => o))
            .ShouldEqual(actual);
    }

    [Fact]
    public async Task 查詢清除流水表單_依照CleanFlowStatus()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

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
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePlayerBucketClient))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var forms = new[]
        {
            new CleanBucketForm(
                Guid.NewGuid(),
                playerCode,
                Guid.NewGuid(),
                Guid.NewGuid(),
                123M,
                new[]
                {
                    new TypeBucketResult(
                        null,
                        new ReadBucketResult(
                            10000M,
                            Guid.NewGuid()))
                },
                Guid.NewGuid(),
                100M,
                124M,
                new[]
                {
                    new TypeBucketResult(
                        null,
                        new ReadBucketResult(
                            11000M,
                            Guid.NewGuid()))
                },
                CleanFlowStatus.Completed,
                DateTime.UtcNow)
        };

        var expected = forms
            .Select(form => new CleanBucketFormViewModel
            {
                Id = form.Id,
                CleaningBalance = form.CleaningBalance,
                CleaningBucketAmount = form.CleaningBucketAmounts
                    ?.FirstOrDefault(result => result.BucketType is null)
                    ?.Result
                    .Amount,
                CreatedOn = form.CreatedOn,
                DepositFormId = form.DepositFormId,
                PlayerCode = form.PlayerCode,
                ReviewBalance = form.CurrentBalance,
                ReviewBucketAmount = form.CurrentBucketAmounts
                    .First()
                    .Result
                    .Amount,
                Status = form.Status.ToString()
            })
            .ToArray();

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo());

        _ = fakePlayerBucketClient.QueryCleanBucketFomsAsync(
            Arg.Is(playerCode),
            Arg.Is(CleanFlowStatus.Completed),
            Arg.Any<CancellationToken>())
            .Returns(forms.ToAsyncEnumerable());

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, flowStatus) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "true"
                        : "false")}});

                let svc = new BackOfficeService(instance);

                return await svc.queryCleanBucketForms(playerCode, flowStatus);
            };
        """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<CleanBucketFormViewModel[]>(
            nodeModule,
            args: new object[] { playerCode, CleanFlowStatus.Completed });

        // Assert
        expected.ToExpectedObject(ctx => ctx
            .ArrayMemberShouldMatch(o => o)).ShouldEqual(actual);
    }

    [Fact]
    public async Task 清除玩家流水()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

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
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePlayerBucketClient))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var brandId = Guid.NewGuid();
        (var anchorId, var anchorId2, var anchorId3) = (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode,
                BrandId = brandId
            });

        _ = fakePlayerBucketClient.GetBrandBucketTypesAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new[] { "Game1", "Game2" }.ToAsyncEnumerable());

        _ = fakePlayerBucketClient.ReadBucketsAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new ReadBucketResult(100M, anchorId));
        _ = fakePlayerBucketClient.ReadBucketsAsync(
            Arg.Is($"{playerCode}:Game1"),
            Arg.Any<CancellationToken>())
            .Returns(new ReadBucketResult(100M, anchorId2));
        _ = fakePlayerBucketClient.ReadBucketsAsync(
            Arg.Is($"{playerCode}:Game2"),
            Arg.Any<CancellationToken>())
            .Returns(new ReadBucketResult(100M, anchorId3));


        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "true"
                        : "false")}});

                let svc = new BackOfficeService(instance);

                return await svc.clearPlayerBuckets(playerCode);
            };
        """;



        // Act
        await StaticNodeJSService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { playerCode });

        // Assert
        _ = fakePlayerBucketClient.Received(1)
            .CleanBucketsAsync(
                Arg.Is(playerCode),
                Arg.Is(anchorId),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
        _ = fakePlayerBucketClient.Received(1)
            .CleanBucketsAsync(
                Arg.Is($"{playerCode}:Game1"),
                Arg.Is(anchorId2),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
        _ = fakePlayerBucketClient.Received(1)
            .CleanBucketsAsync(
                Arg.Is($"{playerCode}:Game2"),
                Arg.Is(anchorId3),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
        _ = fakePlayerBucketClient.Received(3)
            .CleanBucketsAsync(
                Arg.Any<string>(),
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 清除流水表單背景作業()
    {
        // Arrange
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakeGameProviderQueryService = Substitute.For<IQueryService<GetPlayerGameProviderBalanceQuery, GetPlayerGameProviderBalanceResponse>>();

        using var web = TestBackendOfficeApplication.Builder
            .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
            .ConfigureServices(services => services
                .AddSingleton(fakePlayerBucketClient)
                .AddSingleton(fakeCashServiceClient)
                .AddSingleton(fakeGameProviderQueryService))
            .Build();

        var sut = new CleanBucketsHandler(
            web.Services.GetRequiredService<ICommandService<ProcessCleanBucketsRequestCommand>>());

        var request = new CleanFormCreatedEvent
        {
            FormId = Guid.NewGuid()
        };

        var anchorBucketId = Guid.NewGuid();

        var form = new CleanBucketForm(
            request.FormId,
            "test1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            0M,
            new[]
            {
                new TypeBucketResult(
                    null,
                    new ReadBucketResult(
                        1000M,
                        anchorBucketId))
            },
            Guid.NewGuid(),
            5M,
            null,
            null,
            CleanFlowStatus.Create,
            DateTime.UtcNow);

        _ = fakePlayerBucketClient.GetCleanBucketFormAsync(Arg.Is(request.FormId.ToGuid()))
            .Returns(form);

        var cleaningBucketResult = new ReadBucketResult(
            1000M,
            Guid.NewGuid());
        _ = fakePlayerBucketClient.ReadBucketsAsync(Arg.Is("test1"))
            .Returns(cleaningBucketResult);

        _ = fakeCashServiceClient.GetBalanceAsync(Arg.Is(form.WalletId))
            .Returns(0M);

        _ = fakeGameProviderQueryService.QueryAsync(Arg.Any<GetPlayerGameProviderBalanceQuery>())
            .Returns(new[]
            {
                new GetPlayerGameProviderBalanceResponse(
                    "Test",
                    "Test",
                    false,
                    false,
                    0M)
            }.ToAsyncEnumerable());

        var expected = new
        {
            CleaningBalance = 0M,
            CleaningBucketAmounts = new[]
            {
                new TypeBucketResult(
                    null,
                    cleaningBucketResult)
            }
        };

        // Act
        await sut.HandleAsync(request.ToByteArray());

        // Assert
        _ = fakePlayerBucketClient.Received(1)
            .CleanBucketsAsync(
                Arg.Is(form.PlayerCode),
                Arg.Is(anchorBucketId),
                Arg.Is(form.DepositFormId.ToString()),
                Arg.Is(default(string)));

        _ = fakePlayerBucketClient.Received(1)
            .CompleteCleanBucketsAsync(
                Arg.Is(form.Id),
                Arg.Is<ReviewCleanBucketRequest<GetPlayerGameProviderBalanceResponse[]>>(
                    req => expected.ToExpectedObject().Matches(req)));
    }

    [Fact]
    public async Task 調整玩家流水_增加流水()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

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
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePlayerBucketClient))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var request = new AdjustPlayerBucketViewModel
        {
            Amount = 1000M
        };

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "true"
                        : "false")}});

                let svc = new BackOfficeService(instance);

                return await svc.adjustIncreasePlayerBucket(playerCode, request);
            };
        """;

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo());

        // Act
        await StaticNodeJSService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { playerCode, request });

        // Assert
        _ = fakePlayerBucketClient.Received(1)
            .CreateBucketAsync(
                Arg.Is(playerCode),
                Arg.Is(request.Amount),
                Arg.Is(BucketType.Adjustment),
                Arg.Is(default(string)),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 調整玩家流水_增加流水_輸入的參數小於0()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

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
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePlayerBucketClient))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var request = new AdjustPlayerBucketViewModel
        {
            Amount = -1000M
        };

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "true"
                        : "false")}});

                let svc = new BackOfficeService(instance);

                try {
                    await svc.adjustIncreasePlayerBucket(playerCode, request)
                } catch(error) {
                    return error.response.status;
                }
            };
        """;

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo());

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] { playerCode, request });

        // Assert
        Assert.Equal("400", actual);
    }
}

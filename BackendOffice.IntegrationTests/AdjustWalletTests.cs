using System.Runtime.InteropServices;
using System.Text.Json;
using ExpectedObjects;
using Jering.Javascript.NodeJS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using Tgs.BrandCertificate.Clients;
using Tgs.CashQueryService.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class AdjustWalletTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public AdjustWalletTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 取得調帳統計資料()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();
        var fakeBrandRelationQueryService = Substitute.For<IQueryService<BrandRelationQuery, BrandRelationInfo>>();

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
                .AddSingleton(fakeCashQueryClient)
                .AddSingleton(fakeBrandRelationQueryService))
            .Build();

        await web.StartAsync();

        var (from, to) = (DateTime.Now.AddDays(-1D), DateTime.Now);
        var brandId1 = Guid.NewGuid();
        var brandCode1 = "Test1";
        var brandId2 = Guid.NewGuid();
        var brandCode2 = "Test2";
        var licenseeId = Guid.NewGuid();
        var licensee = "L1";
        var resellerId = Guid.NewGuid();

        var expectedCondition = new AdjustRecordQueryCondition(
            (from.ToUniversalTime(), to.ToUniversalTime()),
            null,
            null,
            null);

        _ = fakeBrandServiceClient.GetAllBrandCurrenciesAsync(Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandCurrencyInfo(brandId1, "CNY"),
                new BrandCurrencyInfo(brandId2, "CNY")
            }.ToAsyncEnumerable());

        _ = fakeCashQueryClient.QueryPlayerAdjustSummariesAsync(Arg.Is(expectedCondition), Arg.Any<CancellationToken>())
            .Returns(new[] {
                new AdjustSummaryListItem(
                    brandId1,
                    1,
                    2,
                    3,
                    4,
                    5,
                    0,
                    10M,
                    20M,
                    30M,
                    40M,
                    50M,
                    0M),
                new AdjustSummaryListItem(
                    brandId2,
                    5,
                    4,
                    3,
                    2,
                    1,
                    0,
                    50M,
                    40M,
                    30M,
                    20M,
                    10M,
                    0M)
            }.ToAsyncEnumerable());
        _ = fakeCashQueryClient.QueryAgentAdjustSummariesAsync(Arg.Is(expectedCondition), Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new AdjustSummaryListItem(
                    brandId1,
                    0,
                    0,
                    0,
                    0,
                    0,
                    7,
                    0M,
                    0M,
                    0M,
                    0M,
                    0M,
                    70M),
            }.ToAsyncEnumerable());
        _ = fakeBrandRelationQueryService.QueryAsync(Arg.Any<BrandRelationQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] {
                new BrandRelationInfo(
                    brandId1,
                    brandCode1,
                    brandCode1,
                    licenseeId,
                    licensee,
                    licensee,
                    resellerId,
                    string.Empty,
                    string.Empty),
                new BrandRelationInfo(
                    brandId2,
                    brandCode2,
                    brandCode2,
                    licenseeId,
                    licensee,
                    licensee,
                    resellerId,
                    string.Empty,
                    string.Empty)
            }.ToAsyncEnumerable());

        var expected = new[]{
            new AdjustSummaryListItemViewModel
            {
                BrandId = brandId1,
                Brand = brandCode1,
                Licensee = licensee,
                Company = 1,
                Agent = 2,
                System = 3,
                Transfer = 4,
                Bonus = 5,
                AgentOther = 7,
                PlayerAdjustAmount = 150M,
                AgentAdjustAmount = 70M,
                Currency = "CNY"
            },
            new AdjustSummaryListItemViewModel
            {
                BrandId = brandId2,
                Brand = brandCode2,
                Licensee = licensee,
                Company = 5,
                Agent = 4,
                System = 3,
                Transfer = 2,
                Bonus = 1,
                AgentOther = 0,
                PlayerAdjustAmount = 150M,
                Currency = "CNY"
            }
        };

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (resellerId, from, to) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.queryAdjustSummaries(resellerId, from, to);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<AdjustSummaryListItemViewModel[]>(
            nodeModule,
            args: new object[] { resellerId, from, to });

        // Assert
        expected.ToExpectedObject().ShouldEqual(actual);
    }

    [Fact]
    public async Task 查詢代理調帳記錄()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();
        var fakeBrandRelationQueryService = Substitute.For<IQueryService<BrandRelationQuery, BrandRelationInfo>>();
        var fakeAgentClient = Substitute.For<IAgentClient>();

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
                .AddSingleton(fakeCashQueryClient)
                .AddSingleton(fakeBrandRelationQueryService)
                .AddSingleton(fakeAgentClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var brandCode = "Test";
        var queryViewModel = new AdjustRecordQueryViewModel
        {
            From = DateTime.Now.AddDays(-1D),
            To = DateTime.Now,
            UserName = "ttt"
        };
        var agentId = Guid.NewGuid();

        var queryRecords = new[] {
            new AdjustRecordListItem(
                Guid.NewGuid(),
                brandId,
                agentId.ToString(),
                Tgs.CashQueryService.AdjustType.AgentOther,
                10M,
                20M,
                "manager",
                "test123",
                queryViewModel.From,
                Tgs.CashQueryService.UserType.Agent)
        };

        _ = fakeAgentClient.GetAgentInfoByUsernameAsync(
            Arg.Is(new GetAgentInfoByUsernameRequest(brandId, queryViewModel.UserName)),
            Arg.Any<CancellationToken>())
            .Returns(new AgentInfoModel(
                agentId,
                "test",
                null,
                null,
                null,
                null,
                null,
                "aabbcc",
                null,
                DateTimeOffset.Now,
                AgentStatus.Enabled,
                AgentType.Cash,
                AgentLevelType.Agent,
                0M,
                0,
                0M));

        var queryCondition = new AdjustRecordQueryCondition(
            (queryViewModel.From.ToUniversalTime(), queryViewModel.To.ToUniversalTime()),
            queryViewModel.SearchTypes,
            agentId.ToString(),
            queryViewModel.Operator);
        _ = fakeCashQueryClient.CountAgentAdjustRecordsAsync(
            Arg.Is(brandId),
            Arg.Is(queryCondition),
            Arg.Any<CancellationToken>())
            .Returns(1);
        _ = fakeCashQueryClient.QueryAgentAdjustRecordsAsync(
            Arg.Is(brandId),
            Arg.Is(queryCondition),
            Arg.Is(1),
            Arg.Is(0),
            Arg.Any<CancellationToken>())
            .Returns(queryRecords.ToAsyncEnumerable());

        _ = fakeBrandRelationQueryService.QueryAsync(Arg.Any<BrandRelationQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] {
                new BrandRelationInfo(
                    brandId,
                    brandCode,
                    brandCode,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty)
            }.ToAsyncEnumerable());

        _ = fakeAgentClient.GetAgentInfoAsync(
            Arg.Is(new GetAgentInfoRequest(agentId)),
            Arg.Any<CancellationToken>())
            .Returns(new AgentInfoModel(
                agentId,
                "test",
                null,
                null,
                null,
                null,
                null,
                "aabbcc",
                null,
                DateTimeOffset.Now,
                AgentStatus.Enabled,
                AgentType.Cash,
                AgentLevelType.Agent,
                0M,
                0,
                0M));

        var expected = queryRecords.Select(record => new AdjustRecordListItemViewModel
        {
            Id = record.Id,
            AdjustType = record.AdjustType,
            Amount = record.Amount,
            BrandCode = brandCode,
            BrandId = brandId,
            BucketAmount = record.BucketAmount,
            CreatedOn = record.CreatedOn,
            Operator = record.Operator,
            UserName = "test",
            UserType = record.UserType,
            Remark = record.Remark
        }).ToArray();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, codition, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.queryAgentAdjustRecords(brandId, codition, page, rows);
            };
            """;

        // Act
        var responseString = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] {
                brandId,
                new {
                    queryViewModel.From,
                    queryViewModel.To,
                    Type = queryViewModel.SearchTypes,
                    User = queryViewModel.UserName,
                    queryViewModel.Operator
                },
                1,
                10
            });
        var actual = JsonSerializer.Deserialize<PagedResponse<AdjustRecordListItemViewModel>>(
            responseString!,
            web.Services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);

        // Assert
        expected.ToExpectedObject().ShouldEqual(actual?.Items);
    }

    [Fact]
    public async Task 查詢代理調帳記錄如果輸入的UserName找不到AgentInfo則回傳沒有資料()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();
        var fakeBrandRelationQueryService = Substitute.For<IQueryService<BrandRelationQuery, BrandRelationInfo>>();
        var fakeAgentClient = Substitute.For<IAgentClient>();

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
                .AddSingleton(fakeCashQueryClient)
                .AddSingleton(fakeBrandRelationQueryService)
                .AddSingleton(fakeAgentClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var brandCode = "Test";
        var queryViewModel = new AdjustRecordQueryViewModel
        {
            From = DateTime.Now.AddDays(-1D),
            To = DateTime.Now,
            UserName = "ttt"
        };
        var agentId = Guid.NewGuid();

        var queryRecords = new[] {
            new AdjustRecordListItem(
                Guid.NewGuid(),
                brandId,
                agentId.ToString(),
                Tgs.CashQueryService.AdjustType.AgentOther,
                10M,
                20M,
                "manager",
                "test123",
                queryViewModel.From,
                Tgs.CashQueryService.UserType.Agent)
        };

        var queryCondition = new AdjustRecordQueryCondition(
            (queryViewModel.From.ToUniversalTime(), queryViewModel.To.ToUniversalTime()),
            queryViewModel.SearchTypes,
            agentId.ToString(),
            queryViewModel.Operator);
        _ = fakeCashQueryClient.CountAgentAdjustRecordsAsync(
            Arg.Is(brandId),
            Arg.Is(queryCondition),
            Arg.Any<CancellationToken>())
            .Returns(1);
        _ = fakeCashQueryClient.QueryAgentAdjustRecordsAsync(
            Arg.Is(brandId),
            Arg.Is(queryCondition),
            Arg.Is(1),
            Arg.Is(0),
            Arg.Any<CancellationToken>())
            .Returns(queryRecords.ToAsyncEnumerable());

        _ = fakeBrandRelationQueryService.QueryAsync(Arg.Any<BrandRelationQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] {
                new BrandRelationInfo(
                    brandId,
                    brandCode,
                    brandCode,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty)
            }.ToAsyncEnumerable());

        _ = fakeAgentClient.GetAgentInfoAsync(
            Arg.Is(new GetAgentInfoRequest(agentId)),
            Arg.Any<CancellationToken>())
            .Returns(new AgentInfoModel(
                agentId,
                "test",
                null,
                null,
                null,
                null,
                null,
                "aabbcc",
                null,
                DateTimeOffset.Now,
                AgentStatus.Enabled,
                AgentType.Cash,
                AgentLevelType.Agent,
                0M,
                0,
                0M));

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, codition, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.queryAgentAdjustRecords(brandId, codition, page, rows);
            };
            """;

        // Act
        var responseString = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] {
                brandId,
                new {
                    queryViewModel.From,
                    queryViewModel.To,
                    Type = queryViewModel.SearchTypes,
                    User = queryViewModel.UserName,
                    queryViewModel.Operator
                },
                1,
                10
            });
        var actual = JsonSerializer.Deserialize<PagedResponse<AdjustRecordListItemViewModel>>(
            responseString!,
            web.Services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);

        // Assert
        Assert.True(actual!.Valid);
        Assert.Equal(0, actual!.TotalCount);
        Assert.Empty(actual!.Items);
    }

    [Fact]
    public async Task 查詢玩家調帳記錄()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();
        var fakeBrandRelationQueryService = Substitute.For<IQueryService<BrandRelationQuery, BrandRelationInfo>>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

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
                .AddSingleton(fakeCashQueryClient)
                .AddSingleton(fakeBrandRelationQueryService)
                .AddSingleton(fakePlayerServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var brandCode = "Test";
        var queryViewModel = new AdjustRecordQueryViewModel
        {
            From = DateTime.Now.AddDays(-1D),
            To = DateTime.Now,
            UserName = "player1"
        };

        var queryRecords = new[] {
            new AdjustRecordListItem(
                Guid.NewGuid(),
                brandId,
                "test1",
                Tgs.CashQueryService.AdjustType.CompanyAdjust,
                10M,
                20M,
                "manager",
                "test123",
                queryViewModel.From,
                Tgs.CashQueryService.UserType.Player)
        };

        _ = fakePlayerServiceClient.GetPlayerInfoByUserNameAsync(
            Arg.Is(brandId),
            Arg.Is(queryViewModel.UserName),
            Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo
            {
                PlayerCode = "test1",
                UserName = "player1",
                BrandId = brandId
            });

        var queryCondition = new AdjustRecordQueryCondition(
            (queryViewModel.From.ToUniversalTime(), queryViewModel.To.ToUniversalTime()),
            queryViewModel.SearchTypes,
            "test1",
            queryViewModel.Operator);
        _ = fakeCashQueryClient.CountPlayerAdjustRecordsAsync(
            Arg.Is(brandId),
            Arg.Is(queryCondition),
            Arg.Any<CancellationToken>())
            .Returns(1);
        _ = fakeCashQueryClient.QueryPlayerAdjustRecordsAsync(
            Arg.Is(brandId),
            Arg.Is(queryCondition),
            Arg.Is(1),
            Arg.Is(0),
            Arg.Any<CancellationToken>())
            .Returns(queryRecords.ToAsyncEnumerable());

        _ = fakeBrandRelationQueryService.QueryAsync(Arg.Any<BrandRelationQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] {
                new BrandRelationInfo(
                    brandId,
                    brandCode,
                    brandCode,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty)
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient.QueryPlayerInfoListAsync(
            Arg.Is(brandId),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(new[] {
                new PlayerInfo
                {
                    PlayerCode = "test1",
                    BrandId = brandId,
                    UserName = "player1"
                }
            }.ToAsyncEnumerable());

        var expected = queryRecords.Select(record => new AdjustRecordListItemViewModel
        {
            Id = record.Id,
            AdjustType = record.AdjustType,
            Amount = record.Amount,
            BrandCode = brandCode,
            BrandId = brandId,
            BucketAmount = record.BucketAmount,
            CreatedOn = record.CreatedOn,
            Operator = record.Operator,
            UserName = "player1",
            UserType = record.UserType,
            Remark = record.Remark
        }).ToArray();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, codition, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.queryPlayerAdjustRecords(brandId, codition, page, rows);
            };
            """;

        // Act
        var responseString = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] {
                brandId,
                new
                {
                    queryViewModel.From,
                    queryViewModel.To,
                    Type = queryViewModel.SearchTypes,
                    User = queryViewModel.UserName,
                    queryViewModel.Operator
                },
                1,
                10
            });
        var actual = JsonSerializer.Deserialize<PagedResponse<AdjustRecordListItemViewModel>>(
            responseString!,
            web.Services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);

        // Assert
        expected.ToExpectedObject().ShouldEqual(actual?.Items);
    }

    [Fact]
    public async Task 查詢玩家調帳記錄如果輸入的UserName找不到玩家則回傳沒有資料()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();
        var fakeBrandRelationQueryService = Substitute.For<IQueryService<BrandRelationQuery, BrandRelationInfo>>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

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
                .AddSingleton(fakeCashQueryClient)
                .AddSingleton(fakeBrandRelationQueryService)
                .AddSingleton(fakePlayerServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var brandCode = "Test";
        var queryViewModel = new AdjustRecordQueryViewModel
        {
            From = DateTime.Now.AddDays(-1D),
            To = DateTime.Now,
            UserName = "player1"
        };

        var queryRecords = new[] {
            new AdjustRecordListItem(
                Guid.NewGuid(),
                brandId,
                "test1",
                Tgs.CashQueryService.AdjustType.CompanyAdjust,
                10M,
                20M,
                "manager",
                "test123",
                queryViewModel.From,
                Tgs.CashQueryService.UserType.Player)
        };

        var queryCondition = new AdjustRecordQueryCondition(
            (queryViewModel.From.ToUniversalTime(), queryViewModel.To.ToUniversalTime()),
            queryViewModel.SearchTypes,
            "test1",
            queryViewModel.Operator);
        _ = fakeCashQueryClient.CountPlayerAdjustRecordsAsync(
            Arg.Is(brandId),
            Arg.Is(queryCondition),
            Arg.Any<CancellationToken>())
            .Returns(1);
        _ = fakeCashQueryClient.QueryPlayerAdjustRecordsAsync(
            Arg.Is(brandId),
            Arg.Is(queryCondition),
            Arg.Is(1),
            Arg.Is(0),
            Arg.Any<CancellationToken>())
            .Returns(queryRecords.ToAsyncEnumerable());

        _ = fakeBrandRelationQueryService.QueryAsync(Arg.Any<BrandRelationQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] {
                new BrandRelationInfo(
                    brandId,
                    brandCode,
                    brandCode,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty)
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient.QueryPlayerInfoListAsync(
            Arg.Is(brandId),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(new[] {
                new PlayerInfo
                {
                    PlayerCode = "test1",
                    BrandId = brandId,
                    UserName = "player1"
                }
            }.ToAsyncEnumerable());

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, codition, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.queryPlayerAdjustRecords(brandId, codition, page, rows);
            };
            """;

        // Act
        var responseString = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] {
                brandId,
                new
                {
                    queryViewModel.From,
                    queryViewModel.To,
                    Type = queryViewModel.SearchTypes,
                    User = queryViewModel.UserName,
                    queryViewModel.Operator
                },
                1,
                10
            });
        var actual = JsonSerializer.Deserialize<PagedResponse<AdjustRecordListItemViewModel>>(
            responseString!,
            web.Services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);

        // Assert
        Assert.True(actual!.Valid);
        Assert.Equal(0, actual!.TotalCount);
        Assert.Empty(actual!.Items);
    }

    [Fact]
    public async Task 匯出代理調帳記錄()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();
        var fakeBrandRelationQueryService = Substitute.For<IQueryService<BrandRelationQuery, BrandRelationInfo>>();
        var fakeAgentClient = Substitute.For<IAgentClient>();

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
                .AddSingleton(fakeCashQueryClient)
                .AddSingleton(fakeBrandRelationQueryService)
                .AddSingleton(fakeAgentClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var brandCode = "Test";
        var (from, to) = (DateTime.Now.AddDays(-1D), DateTime.Now);
        var agentId = Guid.NewGuid();

        var queryRecords = new[] {
            new AdjustRecordListItem(
                Guid.NewGuid(),
                brandId,
                agentId.ToString(),
                Tgs.CashQueryService.AdjustType.AgentOther,
                10M,
                20M,
                "manager",
                "test123",
                from,
                Tgs.CashQueryService.UserType.Agent)
        };

        _ = fakeCashQueryClient.QueryAgentAdjustRecordsAsync(
            Arg.Is(brandId),
            Arg.Is(new AdjustRecordQueryCondition(
                (from.ToUniversalTime(), to.ToUniversalTime()),
                null,
                null,
                null)),
            Arg.Is(3),
            Arg.Is(0),
            Arg.Any<CancellationToken>())
            .Returns(queryRecords.ToAsyncEnumerable());

        _ = fakeBrandRelationQueryService.QueryAsync(Arg.Any<BrandRelationQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] {
                new BrandRelationInfo(
                    brandId,
                    brandCode,
                    brandCode,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty)
            }.ToAsyncEnumerable());

        _ = fakeAgentClient.GetAgentInfoAsync(Arg.Is(new GetAgentInfoRequest(agentId)), Arg.Any<CancellationToken>())
            .Returns(new AgentInfoModel(
                agentId,
                "agent1",
                null,
                null,
                null,
                null,
                null,
                string.Empty,
                null,
                DateTimeOffset.Now,
                AgentStatus.Enabled,
                AgentType.Cash,
                AgentLevelType.Agent,
                0M,
                0,
                0M));

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, from, to) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.exportAgentAdjustRecords(brandId, from, to);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] {
                brandId,
                from,
                to
            });

        // Assert
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", actual);
    }

    [Fact]
    public async Task 匯出玩家調帳記錄()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();
        var fakeBrandRelationQueryService = Substitute.For<IQueryService<BrandRelationQuery, BrandRelationInfo>>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

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
                .AddSingleton(fakeCashQueryClient)
                .AddSingleton(fakeBrandRelationQueryService)
                .AddSingleton(fakePlayerServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var brandCode = "Test";
        var (from, to) = (DateTime.Now.AddDays(-1D), DateTime.Now);

        var queryRecords = new[] {
            new AdjustRecordListItem(
                Guid.NewGuid(),
                brandId,
                "test1",
                Tgs.CashQueryService.AdjustType.CompanyAdjust,
                10M,
                20M,
                "manager",
                "test123",
                from,
                Tgs.CashQueryService.UserType.Player)
        };

        _ = fakeCashQueryClient.QueryPlayerAdjustRecordsAsync(
            Arg.Is(brandId),
            Arg.Is(new AdjustRecordQueryCondition(
                (from.ToUniversalTime(), to.ToUniversalTime()),
                null,
                null,
                null)),
            Arg.Is(1),
            Arg.Is(0),
            Arg.Any<CancellationToken>())
            .Returns(queryRecords.ToAsyncEnumerable());

        _ = fakeBrandRelationQueryService.QueryAsync(Arg.Any<BrandRelationQuery>(), Arg.Any<CancellationToken>())
            .Returns(new[] {
                new BrandRelationInfo(
                    brandId,
                    brandCode,
                    brandCode,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty)
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient.QueryPlayerInfoListAsync(
            Arg.Is(brandId),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(new[] {
                new PlayerInfo
                {
                    PlayerCode = "test1",
                    BrandId = brandId,
                    UserName = "player1"
                }
            }.ToAsyncEnumerable());

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (brandId, from, to) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.exportPlayerAdjustRecords(brandId, from, to);
            };
            """;

        // Act
        var actual = await StaticNodeJSService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] {
                brandId,
                from,
                to
            });

        // Assert
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", actual);
    }
}

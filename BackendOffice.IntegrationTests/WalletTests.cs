using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.CashQueryService.Client;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportAgent;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;
using Xunit.Abstractions;
using BoEnums = TGS.Services.BackOffice.Domain.Enums;

namespace BackendOffice.IntegrationTests;

public class WalletTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    [Fact]
    public async Task 取得玩家主錢包餘額()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
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
                .AddSingleton(fakeCashServiceClient))
            .Build();

        await web.StartAsync();

        var playerCode = "test1";
        var walletId = Guid.NewGuid();

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["DefaultWalletId"] = walletId.ToString()
            }.ToImmutableDictionary());
        _ = fakeCashServiceClient.GetBalanceAsync(Arg.Is(walletId), Arg.Any<CancellationToken>())
            .Returns(1000M);

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-wallet-service.js').default;

            module.exports = async (playerCode) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerMainWalletBalance(playerCode);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<decimal>(
            nodeModule,
            args: [playerCode]);

        // Assert
        Assert.Equal(1000M, actual);
    }

    [Fact]
    public async Task 查詢玩家帳變紀錄()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
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
                .AddSingleton(fakeCashServiceClient)
                .AddSingleton(fakeCashQueryClient))
            .Build();

        await web.StartAsync();

        var playerCode = "playerCode";
        var defaultWalletId = Guid.Parse("b97b0c64-8e66-47f8-0998-08d9b932b074");
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var page = 1;
        var rows = 20;

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", defaultWalletId.ToString() }
            }.ToImmutableDictionary());

        var transactionId1 = Guid.Parse("ce8a455e-7f01-0000-0001-0000f79e2257");
        var transacationId2 = Guid.Parse("cf29d2e7-7f01-0000-0001-0000f79e2257");

        var transactionLogs = new Tgs.CashService.Client.PagingResponse<TransactionLog>(
            Items: [
                new(
                    transactionId1,
                    DateTime.UtcNow,
                    0,
                    100M,
                    200M,
                    0M,
                    0M,
                    null,
                    null),
                new(
                    transacationId2,
                    DateTime.UtcNow,
                    0,
                    100M,
                    200M,
                    0M,
                    0M,
                    null,
                    null),
            ],
            Page: page,
            TotalCount: 2
        );

        var queryTransTypes = Enum
            .GetValues(typeof(BoEnums.TransactionType))
            .Cast<BoEnums.TransactionType>()
            .Where(x => x is not BoEnums.TransactionType.WithdrawCommit
                and not BoEnums.TransactionType.TransferCommit
                and not BoEnums.TransactionType.Receive
                and not BoEnums.TransactionType.GameProviderFundOutCommit)
            .Select(x => (int)x)
            .ToArray();

        _ = fakeCashServiceClient.QueryTransactionLogsAsync(
            Arg.Is(defaultWalletId),
            Arg.Is(from),
            Arg.Is(to),
            Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)),
            Arg.Any<PagingConditions>(),
            Arg.Any<CancellationToken>())
            .Returns(transactionLogs);

        var transactionIds = transactionLogs.Items.Select(x => x.Id);

        _ = fakeCashQueryClient.QueryPlayerAdjustIncludeInNetProfitRecordsAsync(
                Arg.Any<IEnumerable<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(new PlayerAdjustRecord[] {
                new(transactionId1,true),
                new(transacationId2,false)
            }.ToAsyncEnumerable());

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-wallet-service.js').default;

            module.exports = async (playerCode, request, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerAccountChanges(playerCode, request, page, rows);
            };
        """;

        var request = new AccountChangesQueryViewModel
        {
            AccountChangeType = null,
            From = from,
            To = to
        };

        var expected = new PagedResponse<PlayerAccountChangeReportModel>()
        {
            TotalCount = 2,
            Valid = true,
            Items = [
                new PlayerAccountChangeReportModel
                {
                    TransactionId = transactionId1,
                    IncludeInNetProfit = true,
                    Amount = 100M,
                    Balance = 200M,
                    GameActionId = null,
                    GameActionType = null,
                    RecordedOn = DateTime.UtcNow,
                    Remark = "",
                    TransactionType = BoEnums.TransactionType.Deposit,
                    ExternalTransactionId = ""
                },
                new PlayerAccountChangeReportModel
                {
                    TransactionId = transacationId2,
                    IncludeInNetProfit = false,
                    Amount = 100M,
                    Balance = 200M,
                    GameActionId = null,
                    GameActionType = null,
                    RecordedOn = DateTime.UtcNow,
                    Remark = "",
                    TransactionType = BoEnums.TransactionType.Deposit,
                    ExternalTransactionId = ""
                }
            ]
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [playerCode, request, page, rows]);

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<PlayerAccountChangeReportModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(2, result?.TotalCount);
    }

    [Fact]
    public async Task 查詢代理帳變紀錄()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
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
                .AddSingleton(fakeAgentClient)
                .AddSingleton(fakeCashServiceClient)
                .AddSingleton(fakeCashQueryClient))
            .Build();

        await web.StartAsync();

        var agentId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var page = 1;
        var rows = 20;

        _ = fakeAgentClient.GetAgentInfoAsync(
            Arg.Is(new GetAgentInfoRequest(agentId)),
            Arg.Any<CancellationToken>())
            .Returns(new AgentInfoModel(
                agentId,
                "tester",
                null,
                null,
                null,
                null,
                null,
                "aaa",
                null,
                DateTimeOffset.Now,
                Tgs.Agent.Client.Models.Enums.AgentStatus.Enabled,
                Tgs.Agent.Client.Models.Enums.AgentType.Cash,
                Tgs.Agent.Client.Models.Enums.AgentLevelType.Agent,
                0M,
                0,
                0M,
                brandId,
                Guid.NewGuid(),
                walletId,
                false,
                true));

        var transactionId1 = Guid.Parse("ce8a455e-7f01-0000-0001-0000f79e2257");
        var transacationId2 = Guid.Parse("cf29d2e7-7f01-0000-0001-0000f79e2257");

        var transactionLogs = new Tgs.CashService.Client.PagingResponse<TransactionLog>(
            Items: [
                new(
                    transactionId1,
                    DateTime.UtcNow,
                    0,
                    100M,
                    200M,
                    0M,
                    0M,
                    null,
                    null),
                new(
                    transacationId2,
                    DateTime.UtcNow,
                    0,
                    100M,
                    200M,
                    0M,
                    0M,
                    null,
                    null),
            ],
            Page: page,
            TotalCount: 2
        );

        var queryTransTypes = Enum
            .GetValues(typeof(BoEnums.TransactionType))
            .Cast<BoEnums.TransactionType>()
            .Where(x => x is not BoEnums.TransactionType.WithdrawCommit
                and not BoEnums.TransactionType.TransferCommit
                and not BoEnums.TransactionType.Receive
                and not BoEnums.TransactionType.GameProviderFundOutCommit)
            .Select(x => (int)x)
            .ToArray();

        _ = fakeCashServiceClient.QueryTransactionLogsAsync(
            Arg.Is(walletId),
            Arg.Is(from),
            Arg.Is(to),
            Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)),
            Arg.Any<PagingConditions>(),
            Arg.Any<CancellationToken>())
            .Returns(transactionLogs);

        var transactionIds = transactionLogs.Items.Select(x => x.Id);

        _ = fakeCashQueryClient.QueryPlayerAdjustIncludeInNetProfitRecordsAsync(
                Arg.Any<IEnumerable<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(new PlayerAdjustRecord[] {
                new(transactionId1,true),
                new(transacationId2,false)
            }.ToAsyncEnumerable());

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-wallet-service.js').default;

            module.exports = async (agentId, request, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getAgentAccountChanges(agentId, request, page, rows);
            };
        """;

        var request = new AccountChangesQueryViewModel
        {
            AccountChangeType = null,
            From = from,
            To = to
        };

        var expected = new PagedResponse<PlayerAccountChangeReportModel>()
        {
            TotalCount = 2,
            Valid = true,
            Items = [
                new PlayerAccountChangeReportModel
                {
                    TransactionId = transactionId1,
                    IncludeInNetProfit = true,
                    Amount = 100M,
                    Balance = 200M,
                    GameActionId = null,
                    GameActionType = null,
                    RecordedOn = DateTime.UtcNow,
                    Remark = "",
                    TransactionType = BoEnums.TransactionType.Deposit,
                    ExternalTransactionId = ""
                },
                new PlayerAccountChangeReportModel
                {
                    TransactionId = transacationId2,
                    IncludeInNetProfit = false,
                    Amount = 100M,
                    Balance = 200M,
                    GameActionId = null,
                    GameActionType = null,
                    RecordedOn = DateTime.UtcNow,
                    Remark = "",
                    TransactionType = BoEnums.TransactionType.Deposit,
                    ExternalTransactionId = ""
                }
            ]
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [agentId, request, page, rows]);

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<PlayerAccountChangeReportModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(2, result?.TotalCount);
    }

    [Fact]
    public async Task 查詢玩家帳變紀錄_舊版()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
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
                .AddSingleton(fakeCashServiceClient)
                .AddSingleton(fakeCashQueryClient))
            .Build();

        await web.StartAsync();

        var playerCode = "playerCode";
        var defaultWalletId = Guid.Parse("b97b0c64-8e66-47f8-0998-08d9b932b074");
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var page = 1;
        var rows = 20;
        var sortby = "recordedOn";
        var orderby = SortOrder.Ascending;
        var showAll = false;

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", defaultWalletId.ToString() }
            }.ToImmutableDictionary());

        var transactionId1 = Guid.Parse("ce8a455e-7f01-0000-0001-0000f79e2257");
        var transacationId2 = Guid.Parse("cf29d2e7-7f01-0000-0001-0000f79e2257");

        var transactionLogs = new Tgs.CashService.Client.PagingResponse<TransactionLog>(
            Items: [
                new(
                    transactionId1,
                    DateTime.UtcNow,
                    0,
                    100M,
                    200M,
                    0M,
                    0M,
                    null,
                    null),
                new(
                    transacationId2,
                    DateTime.UtcNow,
                    0,
                    100M,
                    200M,
                    0M,
                    0M,
                    null,
                    null),
            ],
            Page: page,
            TotalCount: 2
        );

        var queryTransTypes = Enum.GetValues(typeof(BoEnums.TransactionType))
            .Cast<BoEnums.TransactionType>()
            .Where(x => x is not BoEnums.TransactionType.WithdrawCommit
                and not BoEnums.TransactionType.TransferCommit
                and not BoEnums.TransactionType.Receive
                and not BoEnums.TransactionType.GameProviderFundOutCommit)
            .Select(x => (int)x)
            .ToArray();

        _ = fakeCashServiceClient.QueryTransactionLogsAsync(
            Arg.Is(defaultWalletId),
            Arg.Is(from),
            Arg.Is(to),
            Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)),
            Arg.Any<PagingConditions>(),
            Arg.Any<CancellationToken>())
            .Returns(transactionLogs);

        var transactionIds = transactionLogs.Items.Select(x => x.Id);

        _ = fakeCashQueryClient.QueryPlayerAdjustIncludeInNetProfitRecordsAsync(
                Arg.Any<IEnumerable<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(new PlayerAdjustRecord[] {
                new(transactionId1,true),
                new(transacationId2,false)
            }.ToAsyncEnumerable());

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-wallet-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerAccountChangesOld(request);
            };
        """;

        var request = new GetPlayerAccountChangesReportRequest
        {
            AccountChangeType = null,
            From = from,
            To = to,
            PlayerCode = playerCode,
            BrandId = brandId,
            OrderBy = orderby,
            Page = page,
            Rows = rows,
            SortBy = sortby,
            ShowAll = showAll
        };

        var expected = new PagedResponse<PlayerAccountChangeReportModel>()
        {
            TotalCount = 2,
            Valid = true,
            Items = [
                new PlayerAccountChangeReportModel
                {
                    TransactionId = transactionId1,
                    IncludeInNetProfit = true,
                    Amount = 100M,
                    Balance = 200M,
                    GameActionId = null,
                    GameActionType = null,
                    RecordedOn = DateTime.UtcNow,
                    Remark = "",
                    TransactionType = BoEnums.TransactionType.Deposit,
                    ExternalTransactionId = ""
                },
                new PlayerAccountChangeReportModel
                {
                    TransactionId = transacationId2,
                    IncludeInNetProfit = false,
                    Amount = 100M,
                    Balance = 200M,
                    GameActionId = null,
                    GameActionType = null,
                    RecordedOn = DateTime.UtcNow,
                    Remark = "",
                    TransactionType = BoEnums.TransactionType.Deposit,
                    ExternalTransactionId = ""
                }
            ]
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [request]);

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<PlayerAccountChangeReportModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(2, result?.TotalCount);
    }

    [Fact]
    public async Task 查詢代理帳變紀錄_舊版()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeAgentServiceClient = Substitute.For<IAgentClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
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
                .AddSingleton(fakeAgentServiceClient)
                .AddSingleton(fakeCashServiceClient)
                .AddSingleton(fakeCashQueryClient))
            .Build();

        await web.StartAsync();

        var agentId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var page = 1;
        var rows = 20;
        var sortby = "recordedOn";
        var showAll = false;

        _ = fakeAgentServiceClient.GetAgentInfoAsync(
            Arg.Is(new GetAgentInfoRequest(agentId)),
            Arg.Any<CancellationToken>())
            .Returns(new AgentInfoModel(
                agentId,
                "tester",
                null,
                null,
                null,
                null,
                null,
                "aaa",
                null,
                DateTimeOffset.Now,
                Tgs.Agent.Client.Models.Enums.AgentStatus.Enabled,
                Tgs.Agent.Client.Models.Enums.AgentType.Cash,
                Tgs.Agent.Client.Models.Enums.AgentLevelType.Agent,
                0M,
                0,
                0M,
                brandId,
                Guid.NewGuid(),
                walletId,
                false,
                true));

        var transactionId1 = Guid.Parse("ce8a455e-7f01-0000-0001-0000f79e2257");
        var transacationId2 = Guid.Parse("cf29d2e7-7f01-0000-0001-0000f79e2257");

        var transactionLogs = new Tgs.CashService.Client.PagingResponse<TransactionLog>(
            Items: [
                new(
                    transactionId1,
                    DateTime.UtcNow,
                    0,
                    100M,
                    200M,
                    0M,
                    0M,
                    null,
                    null),
                new(
                    transacationId2,
                    DateTime.UtcNow,
                    0,
                    100M,
                    200M,
                    0M,
                    0M,
                    null,
                    null),
            ],
            Page: page,
            TotalCount: 2
        );

        var queryTransTypes = Enum.GetValues(typeof(BoEnums.TransactionType))
            .Cast<BoEnums.TransactionType>()
            .Where(x => x is not BoEnums.TransactionType.WithdrawCommit
                and not BoEnums.TransactionType.TransferCommit
                and not BoEnums.TransactionType.Receive
                and not BoEnums.TransactionType.GameProviderFundOutCommit)
            .Select(x => (int)x)
            .ToArray();

        _ = fakeCashServiceClient.QueryTransactionLogsAsync(
            Arg.Is(walletId),
            Arg.Is(from),
            Arg.Is(to),
            Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)),
            Arg.Any<PagingConditions>(),
            Arg.Any<CancellationToken>())
            .Returns(transactionLogs);

        var transactionIds = transactionLogs.Items.Select(x => x.Id);

        _ = fakeCashQueryClient.QueryPlayerAdjustIncludeInNetProfitRecordsAsync(
            Arg.Any<IEnumerable<Guid>>(),
            Arg.Any<CancellationToken>())
            .Returns(new PlayerAdjustRecord[] {
                new(transactionId1,true),
                new(transacationId2,false)
            }.ToAsyncEnumerable());

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-wallet-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getAgentAccountChangesOld(request);
            };
        """;

        var request = new GetAgentAccountChangesReportRequest
        {
            From = from,
            To = to,
            AgentId = agentId,
            BrandId = brandId,
            Page = page,
            Rows = rows,
            SortBy = sortby,
            ShowAll = showAll
        };

        var expected = new PagedResponse<AgentAccountChangeReportModel>()
        {
            TotalCount = 2,
            Valid = true,
            Items = [
                new AgentAccountChangeReportModel
                {
                    TransactionId = transactionId1,
                    Amount = 100M,
                    Balance = 200M,
                    RecordedOn = DateTime.UtcNow,
                    TransactionType = BoEnums.TransactionType.Deposit,
                },
                new AgentAccountChangeReportModel
                {
                    TransactionId = transacationId2,
                    Amount = 100M,
                    Balance = 200M,
                    RecordedOn = DateTime.UtcNow,
                    TransactionType = BoEnums.TransactionType.Deposit,
                }
            ]
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [request]);

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<PlayerAccountChangeReportModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(2, result?.TotalCount);
    }

    [Fact]
    public async Task 查詢玩家帳變紀錄總金額()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
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
                .AddSingleton(fakeCashServiceClient))
            .Build();

        await web.StartAsync();

        var playerCode = "playerCode";
        var defaultWalletId = Guid.Parse("b97b0c64-8e66-47f8-0998-08d9b932b074");
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", defaultWalletId.ToString() }
            }.ToImmutableDictionary());

        var queryTransTypes = Enum.GetValues(typeof(BoEnums.TransactionType))
            .Cast<BoEnums.TransactionType>()
            .Where(x => x is not BoEnums.TransactionType.WithdrawCommit
                and not BoEnums.TransactionType.TransferCommit
                and not BoEnums.TransactionType.Receive)
            .Select(x => (int)x)
            .ToArray();

        _ = fakeCashServiceClient.QueryTransactionTotalAmountAsync(
            Arg.Is(defaultWalletId),
            Arg.Is(from),
            Arg.Is(to),
            Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)),
            Arg.Any<CancellationToken>())
            .Returns(168000000M);

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-wallet-service.js').default;

            module.exports = async (playerCode, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerAccountChangesTotalAmount(playerCode, request);
            };
        """;

        var request = new AccountChangesQueryViewModel
        {
            AccountChangeType = null,
            From = from,
            To = to
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<decimal>(
            nodeModule,
            args: [playerCode, request]);

        // Assert
        Assert.Equal(168000000M, actual);
    }

    [Fact]
    public async Task 查詢玩家帳變紀錄總金額_舊版()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
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
                .AddSingleton(fakeCashServiceClient))
            .Build();

        await web.StartAsync();

        var playerCode = "playerCode";
        var defaultWalletId = Guid.Parse("b97b0c64-8e66-47f8-0998-08d9b932b074");
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", defaultWalletId.ToString() }
            }.ToImmutableDictionary());

        var queryTransTypes = Enum.GetValues(typeof(BoEnums.TransactionType))
            .Cast<BoEnums.TransactionType>()
            .Where(x => x is not BoEnums.TransactionType.WithdrawCommit
                and not BoEnums.TransactionType.TransferCommit
                and not BoEnums.TransactionType.Receive)
            .Select(x => (int)x)
            .ToArray();

        _ = fakeCashServiceClient.QueryTransactionTotalAmountAsync(
            Arg.Is(defaultWalletId),
            Arg.Is(from),
            Arg.Is(to),
            Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(queryTransTypes)),
            Arg.Any<CancellationToken>())
            .Returns(168000000M);

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-wallet-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerAccountChangesTotalAmountOld(request);
            };
        """;

        var request = new GetPlayerAccountChangesReportTotalAmountRequest
        {
            AccountChangeType = null,
            From = from,
            To = to,
            PlayerCode = playerCode,
            BrandId = brandId,
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [request]);

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PlayerAccountChangesTotalModel>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.Equal(168000000M, result!.Amount);
    }

    [Fact]
    public async Task 查詢玩家未寫回資料庫的帳變記錄()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();

        using var web = TestBackendOfficeApplication.Builder
            .UseTestChannelServer(channelName)
            .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
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
                .AddSingleton(fakeCashServiceClient)
                .AddSingleton(fakeCashQueryClient))
            .Build();

        await web.StartAsync();

        var playerCode = "playerCode";
        var defaultWalletId = Guid.Parse("b97b0c64-8e66-47f8-0998-08d9b932b074");
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var page = 1;
        var rows = 20;

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(
            Arg.Is(playerCode),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                { "DefaultWalletId", defaultWalletId.ToString() }
            }.ToImmutableDictionary());

        var transactionId1 = Guid.Parse("ce8a455e-7f01-0000-0001-0000f79e2257");
        var transacationId2 = Guid.Parse("cf29d2e7-7f01-0000-0001-0000f79e2257");

        var transactionLogs = new Tgs.CashService.Client.PagingResponse<TransactionLog>(
            Items: [
                new(
                    transactionId1,
                    DateTime.UtcNow,
                    0,
                    100M,
                    200M,
                    0M,
                    0M,
                    null,
                    null),
                new(
                    transacationId2,
                    DateTime.UtcNow,
                    0,
                    100M,
                    200M,
                    0M,
                    0M,
                    null,
                    null),
            ],
            Page: page,
            TotalCount: 2
        );

        var queryTransTypes = Enum
            .GetValues(typeof(BoEnums.TransactionType))
            .Cast<BoEnums.TransactionType>()
            .Where(x => x is not BoEnums.TransactionType.WithdrawCommit
                and not BoEnums.TransactionType.TransferCommit)
            .Select(x => (int)x)
            .ToArray();

        _ = fakeCashServiceClient.QueryInRedisTransactionLogsAsync(
            Arg.Is(defaultWalletId),
            Arg.Any<PagingConditions>(),
            Arg.Any<CancellationToken>())
            .Returns(transactionLogs);

        var transactionIds = transactionLogs.Items.Select(x => x.Id);

        _ = fakeCashQueryClient.QueryPlayerAdjustIncludeInNetProfitRecordsAsync(
                Arg.Any<IEnumerable<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(new PlayerAdjustRecord[] {
                new(transactionId1,true),
                new(transacationId2,false)
            }.ToAsyncEnumerable());

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-wallet-service.js').default;

            module.exports = async (playerCode, request, page, rows) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getPlayerWaitPersistentAccountChanges(playerCode, request, page, rows);
            };
        """;

        var request = new AccountChangesQueryViewModel
        {
            AccountChangeType = null,
            From = from,
            To = to
        };

        var expected = new PagedResponse<PlayerAccountChangeReportModel>()
        {
            TotalCount = 2,
            Valid = true,
            Items = [
                new PlayerAccountChangeReportModel
                {
                    TransactionId = transactionId1,
                    IncludeInNetProfit = true,
                    Amount = 100M,
                    Balance = 200M,
                    GameActionId = null,
                    GameActionType = null,
                    RecordedOn = DateTime.UtcNow,
                    Remark = "",
                    TransactionType = BoEnums.TransactionType.Deposit,
                    ExternalTransactionId = ""
                },
                new PlayerAccountChangeReportModel
                {
                    TransactionId = transacationId2,
                    IncludeInNetProfit = false,
                    Amount = 100M,
                    Balance = 200M,
                    GameActionId = null,
                    GameActionType = null,
                    RecordedOn = DateTime.UtcNow,
                    Remark = "",
                    TransactionType = BoEnums.TransactionType.Deposit,
                    ExternalTransactionId = ""
                }
            ]
        };

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: [playerCode, request, page, rows]);

        // Assert
        Assert.NotNull(actual);

        var jsonOptions = web.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var result = JsonSerializer.Deserialize<PagedResponse<PlayerAccountChangeReportModel>>(
            actual!,
            jsonOptions.JsonSerializerOptions);

        Assert.True(result?.Valid);
        Assert.Equal(2, result?.TotalCount);
    }
}

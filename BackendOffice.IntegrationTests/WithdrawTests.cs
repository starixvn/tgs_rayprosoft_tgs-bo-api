using System.Collections.Immutable;
using System.Runtime.InteropServices;
using ExpectedObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.Withdraw;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Commands.Withdraw;
using TGS.Services.BackOffice.Application.Queries.Withdraw;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class WithdrawTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    [Fact]
    public async Task 建立玩家提款單()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCheckIdentityService = Substitute.For<IGetService<CheckPlayerIdentityCommand, bool>>();

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
                .AddSingleton(fakeCheckIdentityService)
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        var request = new CreateWithdrawRequest
        {
            Amount = 1000M,
            BankInfoId = Guid.NewGuid(),
            BrandId = brandId,
            PlayerCode = "test1"
        };

        var paymentChannelId = Guid.NewGuid();

        _ = fakePaymentServiceClient.GetPlayerBankInfosAsync(
            Arg.Is(request.PlayerCode),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new PlayerBankInfo
                {
                    Id = request.BankInfoId,
                    PaymentChannelId = paymentChannelId
                }
            }.ToAsyncEnumerable());
        _ = fakePaymentServiceClient.GetBrandPaymentsAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandPaymentInfo
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    ChannelId = paymentChannelId
                }
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(
            Arg.Is(request.PlayerCode),
            Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = request.PlayerCode,
                BrandId = brandId,
                UserName = "user1"
            });

        _ = fakeCheckIdentityService.GetAsync(
            Arg.Any<CheckPlayerIdentityCommand>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var expectedRequest = new WithdrawFormCreateInfo
        {
            Amount = request.Amount,
            BankInfoId = request.BankInfoId,
            BrandId = request.BrandId,
            UserId = request.PlayerCode,
            UserName = "user1",
            UserType = Tgs.Payment.Clients.UserType.Player,
            Note = request.Note
        };

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-payment-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.createPlayerWithdraw(request);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] {
                request
            });

        // Assert
        _ = fakePaymentServiceClient.Received(1)
            .CreateWithdrawFormAsync(
                Arg.Is<WithdrawFormCreateInfo>(info => expectedRequest.ToExpectedObject().Equals(info)),
                Arg.Any<CancellationToken>());
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Any<Guid>(),
                Arg.Is(PaymentFlowState.WaitApprove),
                cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 建立玩家提款單時_超過VIP提款限額()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCheckIdentityService = Substitute.For<IGetService<CheckPlayerIdentityCommand, bool>>();

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
                .AddSingleton(fakeCheckIdentityService)
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();

        var request = new CreateWithdrawRequest
        {
            Amount = 1000M,
            BankInfoId = Guid.NewGuid(),
            BrandId = brandId,
            PlayerCode = "test1"
        };

        var paymentChannelId = Guid.NewGuid();

        _ = fakePaymentServiceClient.GetPlayerBankInfosAsync(
            Arg.Is(request.PlayerCode),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new PlayerBankInfo
                {
                    Id = request.BankInfoId,
                    PaymentChannelId = paymentChannelId
                }
            }.ToAsyncEnumerable());
        _ = fakePaymentServiceClient.GetBrandPaymentsAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new BrandPaymentInfo
                {
                    Id = Guid.NewGuid(),
                    BrandId = brandId,
                    ChannelId = paymentChannelId
                }
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(
            Arg.Is(request.PlayerCode),
            Arg.Any<CancellationToken>())
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = request.PlayerCode,
                BrandId = brandId,
                UserName = "user1"
            });

        _ = fakeCheckIdentityService.GetAsync(
            Arg.Any<CheckPlayerIdentityCommand>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var expectedRequest = new WithdrawFormCreateInfo
        {
            Amount = request.Amount,
            BankInfoId = request.BankInfoId,
            BrandId = request.BrandId,
            UserId = request.PlayerCode,
            UserName = "user1",
            UserType = Tgs.Payment.Clients.UserType.Player,
            Note = request.Note
        };

        _ = fakePaymentServiceClient.CreateWithdrawFormAsync(
            Arg.Is<WithdrawFormCreateInfo>(info => expectedRequest.ToExpectedObject().Equals(info)),
            Arg.Any<CancellationToken>())
            .Throws(new WithdrawLimitExceedException(null, 10000M));

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-payment-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                try {
                    return await svc.createPlayerWithdraw(request);
                }
                catch(error) {
                    return error.response.status;
                }
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] {
                request
            });

        // Assert
        Assert.Equal("500", actual);
    }

    [Fact]
    public async Task 匯出提款單列表()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakeWithdrawQueryService = Substitute.For<IGetService<WithdrawFormQuery, WithdrawForms>>();
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

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
                .AddSingleton(fakeWithdrawQueryService)
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var (from, to) = (DateTime.Now.AddDays(-1D), DateTime.Now);
        var paymentMethodId = Guid.NewGuid();

        _ = fakeWithdrawQueryService.GetAsync(
            Arg.Any<WithdrawFormQuery>(),
            Arg.Any<CancellationToken>())
            .Returns(new WithdrawForms
            {
                TotalCount = 1,
                Page = 1,
                Rows = 10,
                Sum = 100M,
                Items = new[]
                {
                    new WithdrawFormInfo
                    {
                        Id = Guid.NewGuid(),
                        Amount = 100M,
                        BrandId = brandId
                    }
                }
            });

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-payment-service.js').default;

            module.exports = async (brandId, request, paging) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.exportWithdraw(brandId, request, paging);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<string>(
            nodeModule,
            args: new object[] {
                brandId,
                new(),
                new PageableRequest()
            });

        // Assert
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", actual);
    }

    [Fact]
    public async Task 審核提款單()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCheckIdentityService = Substitute.For<IGetService<CheckPlayerIdentityCommand, bool>>();

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
                .AddSingleton(fakeCheckIdentityService)
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var request = new WithdrawReviewRequest
        {
            BrandId = brandId,
            BrandPaymentId = Guid.NewGuid(),
            ReviewResponse = ReviewResponseState.Approve
        };

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-payment-service.js').default;

            module.exports = async (formId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.reviewWithdrawForm(formId, request);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] {
                formId,
                request
            });

        // Assert
        _ = fakePaymentServiceClient.Received(1)
            .ReviewWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Is(request.BrandPaymentId),
                Arg.Is(ReviewState.Approve),
                Arg.Is(userId.ToString()),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 拒絕提款單()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCheckIdentityService = Substitute.For<IGetService<CheckPlayerIdentityCommand, bool>>();

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
                .AddSingleton(fakeCheckIdentityService)
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var request = new WithdrawReviewRequest
        {
            BrandId = brandId,
            BrandPaymentId = Guid.NewGuid(),
            ReviewResponse = ReviewResponseState.Reject,
            PlayerRemark = "playerRemark"
        };

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-payment-service.js').default;

            module.exports = async (formId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.reviewWithdrawForm(formId, request);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] {
                formId,
                request
            });

        // Assert
        _ = fakePaymentServiceClient.Received(1)
            .ReviewWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Is(request.BrandPaymentId),
                Arg.Is(ReviewState.Reject),
                Arg.Is(userId.ToString()),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Is("playerRemark"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 設定提款單已支付()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakeCheckIdentityService = Substitute.For<IGetService<CheckPlayerIdentityCommand, bool>>();

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
                .AddSingleton(fakeCheckIdentityService)
                .AddSingleton(fakePlayerServiceClient)
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        await web.StartAsync();

        var brandId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var request = new WithdrawReviewRequest
        {
            BrandId = brandId,
            BrandPaymentId = Guid.NewGuid(),
            ReviewResponse = ReviewResponseState.Paid,
            PlayerRemark = "playerRemark"
        };

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-payment-service.js').default;

            module.exports = async (formId, request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.reviewWithdrawForm(formId, request);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<MessageResponse>(
            nodeModule,
            args: new object[] {
                formId,
                request
            });

        // Assert
        _ = fakePaymentServiceClient.Received(1)
            .ReviewWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Is(request.BrandPaymentId),
                Arg.Is(ReviewState.Paid),
                Arg.Is(userId.ToString()),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Is("playerRemark"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 查詢提款單細項()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

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
                .AddSingleton(fakePaymentServiceClient))
            .Build();

        var brandId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        _ = fakePaymentServiceClient
            .GetWithdrawFormAsync(Arg.Is(brandId), Arg.Is(formId), Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                BrandId = brandId,
                Id = formId,
                Amount = 168M,
                UserType = Tgs.Payment.Clients.UserType.Player,
                UserId = "playerCode",
                PlayerRemark = "playerRemark",
            });

        _ = fakePlayerServiceClient
            .GetPlayerPropertiesAsync(Arg.Is("playerCode"), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { { "RealName", "playerName" } }.ToImmutableDictionary());

        await web.StartAsync();

        using var nodeService = nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-payment-service.js').default;

            module.exports = async (brandId, formId) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.getWithdrawFormDetail(brandId, formId);
            };
            """;

        // Act
        var actual = await nodeService.InvokeFromStringAsync<GenericResponse<WithdrawFormDetailViewModel>>(
            nodeModule,
            args: new object[] {
                brandId,
                formId,
            });

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("playerRemark", actual.Data.PlayerRemark);
    }
}

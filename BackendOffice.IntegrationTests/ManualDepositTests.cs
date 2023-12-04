using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.ManualDeposit;
using TGS.Payment.ManualDeposit;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using Xunit.Abstractions;

namespace BackendOffice.IntegrationTests;

public class ManualDepositTests(ITestOutputHelper testOutputHelper, NodeJsFixture nodeJsFixture)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    private readonly NodeJsFixture _nodeJsFixture = nodeJsFixture ?? throw new ArgumentNullException(nameof(nodeJsFixture));

    [Fact]
    public async Task 建立手動EWallet存款單()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var serializer = Substitute.For<ManualDepositSerializer>();
        var fakeGetService = Substitute.For<IGetService<GetBrandPaymentQuery, BrandPaymentInfo>>();
        var fakeCreateService = Substitute.For<IGetService<CreateDepositFormCommand, Guid>>();

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
                .AddSingleton(serializer)
                .AddSingleton(fakeGetService)
                .AddSingleton(fakeCreateService))
            .Build();

        await web.StartAsync();

        var brandPaymentId = Guid.Parse("24d2776f-6adc-45e8-89f7-7dc772aa9008");
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var playerCode = "4ug53rk9xy3q";
        var depositFormId = Guid.NewGuid();
        var amount = 100M;
        var bankType = "bank";
        var realName = "realName";
        var note = "TestNote";
        var promotionId = Guid.NewGuid();

        var request = new SubmitManualDepositEWalletRequest
        {
            BrandId = brandId,
            BrandPaymentId = brandPaymentId,
            Note = note,
            PlayerCode = playerCode,
            PromotionId = promotionId,
            RealName = realName,
            BankType = bankType,
            Amount = amount,
            Phone = "TestPhone"
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.submitDepositEWallet(request);
            };
        """;

        _ = fakeGetService.GetAsync(Arg.Is(new GetBrandPaymentQuery(brandPaymentId)), Arg.Any<CancellationToken>())
            .Returns(new BrandPaymentInfo
            {
                AuthenticateIdentity = false,
                AuthenticatePhone = false,
                ChannelCode = "channelCode",
                ChannelId = Guid.NewGuid(),
                ChannelName = "channelName",
                ConditionMatchType = ConditionMatchType.MatchAll,
                Id = brandPaymentId,
                BrandId = brandId,
                PaymentMethodChannelId = Guid.NewGuid(),
                IsAllPlayers = true,
                PaymentId = Guid.NewGuid(),
                DisplayOrder = 1,
                DepositedAmountLimit = new ValueRange<decimal>() { From = 1M, To = decimal.MaxValue },
                Enabled = true,
                MethodId = Guid.NewGuid(),
                TransferLimit = new ValueRange<decimal>() { From = 1M, To = 100000M },
                TransferType = TransferType.Deposit,
                ExtendedData = "{ \"qrCode\": \"qrCode\", \"eMoneyName\": \"eMoneyName\", \"phone\": \"phone\" }"
            }
            );

        // Act
        await nodeService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { request });

        // Assert
        _ = fakeCreateService
            .Received(1)
            .GetAsync(
                Arg.Any<CreateDepositFormCommand>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 建立手動Pulsa存款單()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var serializer = Substitute.For<ManualDepositSerializer>();
        var fakeGetService = Substitute.For<IGetService<GetBrandPaymentQuery, BrandPaymentInfo>>();
        var fakeCreateService = Substitute.For<IGetService<CreateDepositFormCommand, Guid>>();

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
                .AddSingleton(serializer)
                .AddSingleton(fakeGetService)
                .AddSingleton(fakeCreateService))
            .Build();

        await web.StartAsync();

        var brandPaymentId = Guid.Parse("24d2776f-6adc-45e8-89f7-7dc772aa9008");
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var playerCode = "4ug53rk9xy3q";
        var depositFormId = Guid.NewGuid();
        var amount = 100M;
        var bankType = "bank";
        var realName = "realName";
        var note = "TestNote";
        var promotionId = Guid.NewGuid();

        var request = new SubmitManualDepositPulsaRequest
        {
            BrandId = brandId,
            BrandPaymentId = brandPaymentId,
            Note = note,
            PlayerCode = playerCode,
            PromotionId = promotionId,
            RealName = realName,
            BankType = bankType,
            Amount = amount,
            Phone = "TestPhone"
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.submitDepositPulsa(request);
            };
        """;

        _ = fakeGetService.GetAsync(Arg.Is(new GetBrandPaymentQuery(brandPaymentId)), Arg.Any<CancellationToken>())
            .Returns(new BrandPaymentInfo
            {
                AuthenticateIdentity = false,
                AuthenticatePhone = false,
                ChannelCode = "channelCode",
                ChannelId = Guid.NewGuid(),
                ChannelName = "channelName",
                ConditionMatchType = ConditionMatchType.MatchAll,
                Id = brandPaymentId,
                BrandId = brandId,
                PaymentMethodChannelId = Guid.NewGuid(),
                IsAllPlayers = true,
                PaymentId = Guid.NewGuid(),
                DisplayOrder = 1,
                DepositedAmountLimit = new ValueRange<decimal>() { From = 1M, To = decimal.MaxValue },
                Enabled = true,
                MethodId = Guid.NewGuid(),
                TransferLimit = new ValueRange<decimal>() { From = 1M, To = 100000M },
                TransferType = TransferType.Deposit,
                ExtendedData = "{ \"phone\": \"phone\" }"
            }
            );

        // Act
        await nodeService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { request });

        // Assert
        _ = fakeCreateService
            .Received(1)
            .GetAsync(
                Arg.Any<CreateDepositFormCommand>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 建立手動銀行存款單()
    {
        // Arrange
        var channelName = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var serializer = Substitute.For<ManualDepositSerializer>();
        var fakeGetService = Substitute.For<IGetService<GetBrandPaymentQuery, BrandPaymentInfo>>();
        var fakeCreateService = Substitute.For<IGetService<CreateDepositFormCommand, Guid>>();

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
                .AddSingleton(serializer)
                .AddSingleton(fakeGetService)
                .AddSingleton(fakeCreateService))
            .Build();

        await web.StartAsync();

        var brandPaymentId = Guid.Parse("24d2776f-6adc-45e8-89f7-7dc772aa9008");
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var playerCode = "4ug53rk9xy3q";
        var depositFormId = Guid.NewGuid();
        var amount = 100M;
        var bankAccount = "TestBankAccount";
        var bankName = "TestBankName";
        var bankType = "bank";
        var realName = "realName";
        var note = "TestNote";
        var promotionId = Guid.NewGuid();

        var request = new SubmitManualDepositBankRequest
        {
            BrandId = brandId,
            BrandPaymentId = brandPaymentId,
            Note = note,
            PlayerCode = playerCode,
            PromotionId = promotionId,
            RealName = realName,
            BankType = bankType,
            BankName = bankName,
            BankAccount = bankAccount,
            Amount = amount,
        };

        using var nodeService = _nodeJsFixture.BuildNodeJsService();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

        var nodeModule = $$"""
            const axiosConfigFactory = require('./models/axios-configuration.js').default;
            const BackOfficeService = require('./services/bo-service.js').default;

            module.exports = async (request) =>
            {
                let instance = axiosConfigFactory("{{channelName}}", {{isWindows}});

                let svc = new BackOfficeService(instance);

                return await svc.submitDepositBank(request);
            };
        """;

        _ = fakeGetService.GetAsync(Arg.Is(new GetBrandPaymentQuery(brandPaymentId)), Arg.Any<CancellationToken>())
            .Returns(new BrandPaymentInfo
            {
                AuthenticateIdentity = false,
                AuthenticatePhone = false,
                ChannelCode = "channelCode",
                ChannelId = Guid.NewGuid(),
                ChannelName = "channelName",
                ConditionMatchType = ConditionMatchType.MatchAll,
                Id = brandPaymentId,
                BrandId = brandId,
                PaymentMethodChannelId = Guid.NewGuid(),
                IsAllPlayers = true,
                PaymentId = Guid.NewGuid(),
                DisplayOrder = 1,
                DepositedAmountLimit = new ValueRange<decimal>() { From = 1M, To = decimal.MaxValue },
                Enabled = true,
                MethodId = Guid.NewGuid(),
                TransferLimit = new ValueRange<decimal>() { From = 1M, To = 100000M },
                TransferType = TransferType.Deposit,
                ExtendedData = "{ \"bankAccount\": \"bankAccount\", \"name\": \"name\" }"
            }
            );

        // Act
        await nodeService.InvokeFromStringAsync(
            nodeModule,
            args: new object[] { request });

        // Assert
        _ = fakeCreateService
            .Received(1)
            .GetAsync(
                Arg.Any<CreateDepositFormCommand>(),
                Arg.Any<CancellationToken>());
    }
}

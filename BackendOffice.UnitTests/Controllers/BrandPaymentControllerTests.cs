using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using TGS.Apis.BackendOffice.Controllers.BrandPayment;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Apis.BackendOffice.Services;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class BrandPaymentControllerTests : BaseTests
{
    private readonly Mock<ITgsQueries<BrandPaymentAuditLog>> _auditLogQueriesMock =
        new();

    private readonly Mock<IBrandPaymentService> _paymentServiceMock = new();

    private readonly Mock<ITgsQueries<Player>> _playerQueriesMock =
        new();

    private readonly Mock<IUploadService> _uploadServiceMock = new();

    private BrandPaymentController _controller = default!;

    [Test]
    public async Task Can_Create_Payment()
    {
        var request = new CreateBrandPaymentRequest
        {
            Id = Guid.Empty,
            PaymentMethodChannelId = Guid.NewGuid(),
            PaymentStatus = SwitchStatus.Enabled,
            ChannelName = "Test",
            ExtendedData = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                name = "測試者",
                bankAccount = "9876543210"
            })),
            TransferLimitFrom = 10M,
            TransferLimitTo = 1000M,
            PlayerListCondition = PlayerListCondition.MatchAll,
            AllPlayers = true,
            IdentityAuthentication = false,
            PhoneAuthentication = false,
            DepositedAmountUnlimited = true,
            DepositedAmountMin = 0M,
            DepositedAmountMax = 0M,
            Whitelist = Array.Empty<string>(),
            Blacklist = Array.Empty<string>(),
            VipLevelIds = Array.Empty<Guid>()
        };

        _ = _paymentServiceMock
            .Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<BrandPayment, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.CreateDepositBrandPaymentAsync(request);
        _ = response.Valid.Should().BeTrue();

        _paymentServiceMock.Verify(p => p.CreatePaymentAsync(
            It.Is<CreateBrandPaymentRequest>(r => r.ChannelName == request.ChannelName),
            It.Is<PaymentTransferType>(x => x == PaymentTransferType.Deposit),
            It.Is<string>(r => r == Username)));
    }

    [Test]
    public async Task Can_Update_Payment()
    {
        var payment = CreatePaymentFixture().Create<BrandPayment>();
        var request = new UpdateBrandPaymentRequest
        {
            Id = Guid.Empty,
            PaymentMethodChannelId = Guid.NewGuid(),
            PaymentStatus = SwitchStatus.Enabled,
            ChannelName = "Test",
            ExtendedData = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                name = "測試者",
                bankAccount = "9876543210"
            })),
            TransferLimitFrom = 10M,
            TransferLimitTo = 1000M,
            PlayerListCondition = PlayerListCondition.MatchAll,
            AllPlayers = true,
            IdentityAuthentication = false,
            PhoneAuthentication = false,
            DepositedAmountUnlimited = true,
            DepositedAmountMin = 0M,
            DepositedAmountMax = 0M,
            Whitelist = Array.Empty<string>(),
            Blacklist = Array.Empty<string>(),
            VipLevelIds = Array.Empty<Guid>()
        };

        _ = _paymentServiceMock.Setup(s => s.GetPaymentAsync(It.IsAny<Expression<Func<BrandPayment, bool>>>(),
                It.IsAny<Func<IQueryable<BrandPayment>, IQueryable<BrandPayment>>>(), true))
            .Returns(Task.FromResult(payment));

        _ = _paymentServiceMock.Setup(s => s.ExistsAsync(It.IsAny<Expression<Func<BrandPayment, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.UpdateBrandPaymentAsync(payment.Id, request);

        _ = response.Valid.Should().BeTrue();
        _paymentServiceMock.Verify(s => s.GetPaymentAsync(It.IsAny<Expression<Func<BrandPayment, bool>>>(),
            It.IsAny<Func<IQueryable<BrandPayment>, IQueryable<BrandPayment>>>(), true));
        _paymentServiceMock.Verify(p => p.UpdatePaymentAsync(It.Is<BrandPayment>(r =>
                r.ChannelName == payment.ChannelName),
            It.IsAny<UpdateBrandPaymentRequest>(),
            It.Is<string>(r => r == Username)));
    }

    [Test]
    public async Task Cannot_Create_Payment_With_Whitelist_Same_As_Blacklist()
    {
        var request = new CreateBrandPaymentRequest
        {
            Id = Guid.Empty,
            PaymentMethodChannelId = Guid.NewGuid(),
            PaymentStatus = SwitchStatus.Enabled,
            ChannelName = "Test",
            ExtendedData = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                name = "測試者",
                bankAccount = "9876543210"
            })),
            TransferLimitFrom = 10M,
            TransferLimitTo = 1000M,
            PlayerListCondition = PlayerListCondition.MatchAll,
            AllPlayers = true,
            IdentityAuthentication = false,
            PhoneAuthentication = false,
            DepositedAmountUnlimited = true,
            DepositedAmountMin = 0M,
            DepositedAmountMax = 0M,
            Whitelist = new string[] { "Tester" },
            Blacklist = new string[] { "Tester" },
            VipLevelIds = Array.Empty<Guid>()
        };
        request.Whitelist[0] = request.Blacklist[0];
        var response = await _controller.CreateDepositBrandPaymentAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.DuplicatePlayers[0].Should().Be(request.Whitelist[0]);
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.BrandPayment.WhiteSameAsBlack);
    }

    [Test]
    public async Task Cannot_Update_Payment_When_Not_Found()
    {
        var payment = CreatePaymentFixture().Create<BrandPayment>();
        var request = new UpdateBrandPaymentRequest
        {
            Id = Guid.Empty,
            PaymentMethodChannelId = Guid.NewGuid(),
            PaymentStatus = SwitchStatus.Enabled,
            ChannelName = "Test",
            ExtendedData = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                name = "測試者",
                bankAccount = "9876543210"
            })),
            TransferLimitFrom = 10M,
            TransferLimitTo = 1000M,
            PlayerListCondition = PlayerListCondition.MatchAll,
            AllPlayers = true,
            IdentityAuthentication = false,
            PhoneAuthentication = false,
            DepositedAmountUnlimited = true,
            DepositedAmountMin = 0M,
            DepositedAmountMax = 0M,
            Whitelist = Array.Empty<string>(),
            Blacklist = Array.Empty<string>(),
            VipLevelIds = Array.Empty<Guid>()
        };

        _ = _paymentServiceMock.Setup(s => s.GetPaymentAsync(
                It.IsAny<Expression<Func<BrandPayment, bool>>>(),
                It.IsAny<Func<IQueryable<BrandPayment>,
                    IQueryable<BrandPayment>>>(), true))
            .Returns(Task.FromResult<BrandPayment>(default!));

        _ = _paymentServiceMock.Setup(s =>
                s.ExistsAsync(It.IsAny<Expression<Func<BrandPayment, bool>>>()))
            .Returns(Task.FromResult(false));

        var response = await _controller.UpdateBrandPaymentAsync(payment.Id, request);

        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Apis.BackendOffice.Resources.BrandPayment.NotFound);
        _paymentServiceMock.Verify(s => s.GetPaymentAsync(
            It.IsAny<Expression<Func<BrandPayment, bool>>>(),
            It.IsAny<Func<IQueryable<BrandPayment>,
                IQueryable<BrandPayment>>>(), true));
        _paymentServiceMock.Verify(p => p.UpdatePaymentAsync(It.Is<BrandPayment>(r =>
                r.ChannelName == payment.ChannelName),
            It.IsAny<UpdateBrandPaymentRequest>(),
            It.Is<string>(r => r == Username)), Times.Never);
    }

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new(
            _playerQueriesMock.Object,
            _uploadServiceMock.Object,
            _paymentServiceMock.Object,
            _auditLogQueriesMock.Object);
        _controller.ControllerContext.HttpContext = HttpContextMock.Object;
    }

    private static Fixture CreatePaymentFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<BrandPayment>(u => u
            .With(s => s.Brand, new Brand())
            .With(s => s.BrandPaymentPlayers, Array.Empty<BrandPaymentPlayer>())
            .With(s => s.VipLevels, Array.Empty<BrandPaymentVipLevel>())
            .With(s => s.ExtendedData, "{}"));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}

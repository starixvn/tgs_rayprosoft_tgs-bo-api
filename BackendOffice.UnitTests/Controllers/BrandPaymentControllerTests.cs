using System.Linq.Expressions;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Tgs.Logging;
using TGS.Apis.BackendOffice.Controllers.BrandPayment;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Apis.BackendOffice.Services;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using Tgs.Logging;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class BrandPaymentControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    //[Test]
    //public async Task Can_Create_Payment()
    //{
    //    var auditLogQueriesMock = Substitute.For<ITgsQueries<BrandPaymentAuditLog>>();

    //    var paymentServiceMock = Substitute.For<IBrandPaymentService>();

    //    var playerQueriesMock = Substitute.For<ITgsQueries<Player>>();

    //    var uploadServiceMock = Substitute.For<IUploadService>();

    //    var controller = new BrandPaymentController(playerQueriesMock, uploadServiceMock, paymentServiceMock,
    //        auditLogQueriesMock)
    //    { ControllerContext = { HttpContext = HttpContextMock } };

    //    var request = new CreateBrandPaymentRequest
    //    {
    //        Id = Guid.Empty,
    //        PaymentMethodChannelId = Guid.NewGuid(),
    //        PaymentStatus = SwitchStatus.Enabled,
    //        ChannelName = "Test",
    //        ExtendedData = JsonDocument.Parse(JsonSerializer.Serialize(new
    //        {
    //            name = "測試者",
    //            bankAccount = "9876543210"
    //        })),
    //        TransferLimitFrom = 10M,
    //        TransferLimitTo = 1000M,
    //        PlayerListCondition = PlayerListCondition.MatchAll,
    //        AllPlayers = true,
    //        IdentityAuthentication = false,
    //        PhoneAuthentication = false,
    //        DepositedAmountUnlimited = true,
    //        DepositedAmountMin = 0M,
    //        DepositedAmountMax = 0M,
    //        Whitelist = [],
    //        Blacklist = [],
    //        VipLevelIds = []
    //    };

    //    _ = paymentServiceMock
    //        .ExistsAsync(Arg.Any<Expression<Func<BrandPayment, bool>>>())
    //        .Returns(Task.FromResult(false));

    //    var response = await controller.CreateDepositBrandPaymentAsync(request);

    //    _ = response.Valid.Should().BeTrue();

    //    _ = paymentServiceMock.Received(1).CreatePaymentAsync(
    //        Arg.Is<CreateBrandPaymentRequest>(r => r.ChannelName == request.ChannelName),
    //        Arg.Is<PaymentTransferType>(x => x == PaymentTransferType.Deposit),
    //        Arg.Is<string>(r => r == Username));
    //}

    //[Test]
    //public async Task Can_Update_Payment()
    //{
    //    var auditLogQueriesMock = Substitute.For<ITgsQueries<BrandPaymentAuditLog>>();

    //    var paymentServiceMock = Substitute.For<IBrandPaymentService>();

    //    var playerQueriesMock = Substitute.For<ITgsQueries<Player>>();

    //    var uploadServiceMock = Substitute.For<IUploadService>();

        //var tsgLogMock = Substitute.For<ITgsLog>();

        //var controller = new BrandPaymentController(playerQueriesMock, uploadServiceMock, paymentServiceMock,
        //    auditLogQueriesMock, tsgLogMock)

    //    var payment = CreatePaymentFixture().Create<BrandPayment>();

    //    var request = new UpdateBrandPaymentRequest
    //    {
    //        Id = Guid.Empty,
    //        PaymentMethodChannelId = Guid.NewGuid(),
    //        PaymentStatus = SwitchStatus.Enabled,
    //        ChannelName = "Test",
    //        ExtendedData = JsonDocument.Parse(JsonSerializer.Serialize(new
    //        {
    //            name = "測試者",
    //            bankAccount = "9876543210"
    //        })),
    //        TransferLimitFrom = 10M,
    //        TransferLimitTo = 1000M,
    //        PlayerListCondition = PlayerListCondition.MatchAll,
    //        AllPlayers = true,
    //        IdentityAuthentication = false,
    //        PhoneAuthentication = false,
    //        DepositedAmountUnlimited = true,
    //        DepositedAmountMin = 0M,
    //        DepositedAmountMax = 0M,
    //        Whitelist = [],
    //        Blacklist = [],
    //        VipLevelIds = []
    //    };

    //    _ = paymentServiceMock.GetPaymentAsync(Arg.Any<Expression<Func<BrandPayment, bool>>>(),
    //            Arg.Any<Func<IQueryable<BrandPayment>, IQueryable<BrandPayment>>>(), true)
    //        .Returns(Task.FromResult(payment));

    //    _ = paymentServiceMock.ExistsAsync(Arg.Any<Expression<Func<BrandPayment, bool>>>())
    //        .Returns(Task.FromResult(false));

    //    var response = await controller.UpdateBrandPaymentAsync(payment.Id, request);

    //    _ = response.Valid.Should().BeTrue();

    //    _ = paymentServiceMock.Received(1).GetPaymentAsync(Arg.Any<Expression<Func<BrandPayment, bool>>>(),
    //        Arg.Any<Func<IQueryable<BrandPayment>, IQueryable<BrandPayment>>>(), true);

    //    _ = paymentServiceMock.Received(1).UpdatePaymentAsync(Arg.Is<BrandPayment>(r =>
    //            r.ChannelName == payment.ChannelName),
    //        Arg.Any<UpdateBrandPaymentRequest>(),
    //        Arg.Is<string>(r => r == Username));
    //}

    //[Test]
    //public async Task Cannot_Create_Payment_With_Whitelist_Same_As_Blacklist()
    //{
    //    var auditLogQueriesMock = Substitute.For<ITgsQueries<BrandPaymentAuditLog>>();

    //    var paymentServiceMock = Substitute.For<IBrandPaymentService>();

    //    var playerQueriesMock = Substitute.For<ITgsQueries<Player>>();

    //    var uploadServiceMock = Substitute.For<IUploadService>();

    //    var controller = new BrandPaymentController(playerQueriesMock, uploadServiceMock, paymentServiceMock,
    //        auditLogQueriesMock);

    //    var request = new CreateBrandPaymentRequest
    //    {
    //        Id = Guid.Empty,
    //        PaymentMethodChannelId = Guid.NewGuid(),
    //        PaymentStatus = SwitchStatus.Enabled,
    //        ChannelName = "Test",
    //        ExtendedData = JsonDocument.Parse(JsonSerializer.Serialize(new
    //        {
    //            name = "測試者",
    //            bankAccount = "9876543210"
    //        })),
    //        TransferLimitFrom = 10M,
    //        TransferLimitTo = 1000M,
    //        PlayerListCondition = PlayerListCondition.MatchAll,
    //        AllPlayers = true,
    //        IdentityAuthentication = false,
    //        PhoneAuthentication = false,
    //        DepositedAmountUnlimited = true,
    //        DepositedAmountMin = 0M,
    //        DepositedAmountMax = 0M,
    //        Whitelist =
    //        [
    //            "Tester"
    //        ],
    //        Blacklist =
    //        [
    //            "Tester"
    //        ],
    //        VipLevelIds = []
    //    };

    //    request.Whitelist[0] = request.Blacklist[0];

    //    var response = await controller.CreateDepositBrandPaymentAsync(request);

    //    _ = response.Valid.Should().BeFalse();

    //    _ = response.DuplicatePlayers[0].Should().Be(request.Whitelist[0]);

    //    _ = response.Message.Should().Be(Apis.BackendOffice.Resources.BrandPayment.WhiteSameAsBlack);
    //}

    //[Test]
    //public async Task Cannot_Update_Payment_When_Not_Found()
    //{
    //    var auditLogQueriesMock = Substitute.For<ITgsQueries<BrandPaymentAuditLog>>();

    //    var paymentServiceMock = Substitute.For<IBrandPaymentService>();

    //    var playerQueriesMock = Substitute.For<ITgsQueries<Player>>();

    //    var uploadServiceMock = Substitute.For<IUploadService>();

    //    var controller = new BrandPaymentController(playerQueriesMock, uploadServiceMock, paymentServiceMock,
    //        auditLogQueriesMock)
    //    { ControllerContext = { HttpContext = HttpContextMock } };

    //    var payment = CreatePaymentFixture().Create<BrandPayment>();

    //    var request = new UpdateBrandPaymentRequest
    //    {
    //        Id = Guid.Empty,
    //        PaymentMethodChannelId = Guid.NewGuid(),
    //        PaymentStatus = SwitchStatus.Enabled,
    //        ChannelName = "Test",
    //        ExtendedData = JsonDocument.Parse(JsonSerializer.Serialize(new
    //        {
    //            name = "測試者",
    //            bankAccount = "9876543210"
    //        })),
    //        TransferLimitFrom = 10M,
    //        TransferLimitTo = 1000M,
    //        PlayerListCondition = PlayerListCondition.MatchAll,
    //        AllPlayers = true,
    //        IdentityAuthentication = false,
    //        PhoneAuthentication = false,
    //        DepositedAmountUnlimited = true,
    //        DepositedAmountMin = 0M,
    //        DepositedAmountMax = 0M,
    //        Whitelist = [],
    //        Blacklist = [],
    //        VipLevelIds = []
    //    };

    //    _ = paymentServiceMock.GetPaymentAsync(
    //            Arg.Any<Expression<Func<BrandPayment, bool>>>(),
    //            Arg.Any<Func<IQueryable<BrandPayment>,
    //                IQueryable<BrandPayment>>>(), true)
    //        .Returns(Task.FromResult<BrandPayment>(default!));

    //    _ = paymentServiceMock.ExistsAsync(Arg.Any<Expression<Func<BrandPayment, bool>>>())
    //        .Returns(Task.FromResult(false));

    //    var response = await controller.UpdateBrandPaymentAsync(payment.Id, request);

    //    _ = response.Valid.Should().BeFalse();

    //    _ = response.Message.Should().Be(Apis.BackendOffice.Resources.BrandPayment.NotFound);

    //    _ = paymentServiceMock.Received(1).GetPaymentAsync(
    //        Arg.Any<Expression<Func<BrandPayment, bool>>>(),
    //        Arg.Any<Func<IQueryable<BrandPayment>,
    //            IQueryable<BrandPayment>>>(), true);

    //    _ = paymentServiceMock.Received(0).UpdatePaymentAsync(Arg.Is<BrandPayment>(r =>
    //            r.ChannelName == payment.ChannelName),
    //        Arg.Any<UpdateBrandPaymentRequest>(),
    //        Arg.Is<string>(r => r == Username));
    //}

    //private static Fixture CreatePaymentFixture()
    //{
    //    var fixture = new Fixture();
    //    fixture.Customize<BrandPayment>(u => u
    //        .With(s => s.Brand, new Brand())
    //        .With(s => s.BrandPaymentPlayers, [])
    //        .With(s => s.VipLevels, [])
    //        .With(s => s.ExtendedData, "{}"));
    //    fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    //    return fixture;
    //}
}
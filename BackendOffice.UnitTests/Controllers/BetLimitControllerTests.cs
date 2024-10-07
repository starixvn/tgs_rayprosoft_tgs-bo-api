using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using TGS.Apis.BackendOffice.Controllers.BetLimit;
using TGS.Services.BackOffice.Application.Models.BetLimit;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class BetLimitControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Create_BetLimit()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BetLimitController(mediatorMock);

        var request = new Fixture().Create<CreateBetLimitRequest>();

        _ = mediatorMock
            .Send(Arg.Any<CreateBetLimitRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.CreateBetLimitAsync(request);
        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<CreateBetLimitRequest>(), Arg.Any<CancellationToken>());
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_BetLimits(bool showAll)
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BetLimitController(mediatorMock);

        var list = CreateBetLimitModelFixture()
            .CreateMany<BetLimitModel>(99)
            .ToArray();

        var request = new GetBetLimitsRequest
        {
            Keyword = "keyword",
            Page = 1,
            Rows = 10,
            SortBy = "updatedOn",
            OrderBy = Core.Models.Messages.SortOrder.Descending,
            ShowAll = showAll
        };
        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.UpdatedOn);

        var expectedRoles = showAll ? [.. orderedRoles] : orderedRoles.Skip(skip).Take(take).ToArray();

        _ = mediatorMock
            .Send(Arg.Any<GetBetLimitsRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<BetLimitModel>
            {
                Valid = true,
                Items = expectedRoles
            });

        var response = await controller.GetBetLimitsAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
            _ = response.Items[i].Id.Should().Be(expectedRoles[i].Id);

        _ = mediatorMock.Received(1).Send(Arg.Any<GetBetLimitsRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Update_BetLimit()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BetLimitController(mediatorMock);

        var betLimit = CreateBetLimitModelFixture().Create<BetLimitModel>();
        var request = new Fixture().Create<UpdateBetLimitRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateBetLimitRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.UpdateBetLimitAsync(betLimit.Id, request);

        _ = response.Valid.Should().BeTrue();
        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateBetLimitRequest>(), Arg.Any<CancellationToken>());
    }

    private static Fixture CreateBetLimitModelFixture()
    {
        var fixture = new Fixture();
        _ = fixture.Create<BetLimitModel>();
        return fixture;
    }
}
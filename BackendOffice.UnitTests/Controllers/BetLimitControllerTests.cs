using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using TGS.Apis.BackendOffice.Controllers.BetLimit;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BetLimit;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class BetLimitControllerTests : BaseTests
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new BetLimitController(
            _mediatorMock.Object);
    }

    private BetLimitController _controller = default!;
    private readonly Mock<IMediator> _mediatorMock = new();

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_BetLimits(bool showAll)
    {
        var list = CreateBetLimitModelFixture()
            .CreateMany<BetLimitModel>(99)
            .ToArray();

        var request = new GetBetLimitsRequest
        {
            Keyword = "keyword",
            Page = 1,
            Rows = 10,
            SortBy = "updatedOn",
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };
        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.UpdatedOn);

        var expectedRoles = showAll ? orderedRoles.ToArray() : orderedRoles.Skip(skip).Take(take).ToArray();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetBetLimitsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<BetLimitModel>
            {
                Valid = true,
                Items = expectedRoles
            });

        var response = await _controller.GetBetLimitsAsync(request);
        response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++) response.Items[i].Id.Should().Be(expectedRoles[i].Id);

        _mediatorMock.Verify(x => x.Send(It.IsAny<GetBetLimitsRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Create_BetLimit()
    {
        var request = new Fixture().Create<CreateBetLimitRequest>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateBetLimitRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = true });

        var response = await _controller.CreateBetLimitAsync(request);
        response.Valid.Should().BeTrue();

        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateBetLimitRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Update_BetLimit()
    {
        var betLimit = CreateBetLimitModelFixture().Create<BetLimitModel>();
        var request = new Fixture().Create<UpdateBetLimitRequest>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateBetLimitRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = true });

        var response = await _controller.UpdateBetLimitAsync(betLimit.Id, request);

        response.Valid.Should().BeTrue();
        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateBetLimitRequest>(), It.IsAny<CancellationToken>()));
    }

    private Fixture CreateBetLimitModelFixture()
    {
        var fixture = new Fixture();
        fixture.Create<BetLimitModel>();
        return fixture;
    }
}
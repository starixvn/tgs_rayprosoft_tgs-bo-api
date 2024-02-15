using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using TGS.Apis.BackendOffice.Controllers.Brand;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class BrandGameProviderControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Get_Default_Times()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandGameProviderController(mediatorMock);
        var list = CreateScheduleModelFixture()
            .CreateMany<DefaultTimeModel>(99)
            .ToArray();

        _ = mediatorMock
            .Send(Arg.Any<GetDefaultTimesRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetDefaultTimesResponse
            {
                Valid = true,
                DefaultTimes = list
            });

        var response = await controller.GetBrandGameProviderDefaultTimesAsync(Guid.NewGuid());
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.DefaultTimes.Length; i++)
        {
            _ = response.DefaultTimes[i].From.Should().Be(list[i].From);
            _ = response.DefaultTimes[i].To.Should().Be(list[i].To);
        }

        _ = mediatorMock.Received(1).Send(Arg.Any<GetDefaultTimesRequest>(), Arg.Any<CancellationToken>());
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Schedules(bool showAll)
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandGameProviderController(mediatorMock);

        var list = CreateScheduleModelFixture()
            .CreateMany<BrandGameProviderModel>(99)
            .ToArray();

        var request = new GetBrandGameProvidersRequest
        {
            Keyword = "keyword",
            Page = 1,
            Rows = 10,
            SortBy = "updatedOn",
            OrderBy = Core.Models.Messages.SortOrder.Descending,
            ShowAll = showAll
        };
        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.GameProviderName);

        var expectedRoles = showAll ? orderedRoles.ToArray() : orderedRoles.Skip(skip).Take(take).ToArray();

        _ = mediatorMock
            .Send(Arg.Any<GetBrandGameProvidersRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<BrandGameProviderModel>
            {
                Valid = true,
                Items = expectedRoles
            });

        var response = await controller.GetBrandGameProvidersAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            _ = response.Items[i].GameProviderName.Should().Be(expectedRoles[i].GameProviderName);
        }

        _ = mediatorMock.Received(1).Send(Arg.Any<GetBrandGameProvidersRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Update_Default_Times()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandGameProviderController(mediatorMock);
        var request = new Fixture().Create<UpdateDefaultTimesRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateDefaultTimesRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response =
            await controller.UpdateBrandGameProviderDefaultTimesAsync(request.BrandGameProviderId, request);

        _ = response.Valid.Should().BeTrue();
        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateDefaultTimesRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Update_Schedule()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandGameProviderController(mediatorMock);
        var schedule = CreateScheduleModelFixture().Create<BrandGameProviderModel>();
        var request = new Fixture().Create<UpdateBrandGameProviderRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateBrandGameProviderRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.UpdateBrandGameProviderAsync(schedule.Id, request);

        _ = response.Valid.Should().BeTrue();
        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateBrandGameProviderRequest>(), Arg.Any<CancellationToken>());
    }

    private static Fixture CreateScheduleModelFixture()
    {
        var fixture = new Fixture();
        return fixture;
    }
}
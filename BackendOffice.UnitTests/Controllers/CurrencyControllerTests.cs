using AutoFixture;
using FluentAssertions;
using MediatR;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Services.BackOffice.Application.Models.Currency;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class CurrencyControllerTests : BaseTests
{
    [Test]
    public async Task Can_Create_Currency()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new CurrencyController(mediatorMock);

        var request = new Fixture().Create<CreateCurrencyRequest>();

        _ = mediatorMock
            .Send(Arg.Any<CreateCurrencyRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.CreateCurrencyAsync(request);
        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<CreateCurrencyRequest>(), Arg.Any<CancellationToken>());
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Currencies(bool showAll)
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new CurrencyController(mediatorMock);

        var list = CreateCurrencyModelFixture()
            .CreateMany<CurrencyModel>(99)
            .ToArray();

        var request = new GetCurrenciesRequest
        {
            Code = "Code",
            Name = "Name",
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
            .Send(Arg.Any<GetCurrenciesRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<CurrencyModel>
            {
                Valid = true,
                Items = expectedRoles
            });

        var response = await controller.GetCurrenciesAsync(request);

        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].Code.Should().Be(expectedRoles[i].Code);
            _ = response.Items[i].Name.Should().Be(expectedRoles[i].Name);
        }

        _ = mediatorMock.Received(1).Send(Arg.Any<GetCurrenciesRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Update_Currency()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new CurrencyController(mediatorMock);

        var currency = CreateCurrencyModelFixture().Create<CurrencyModel>();

        var request = new Fixture().Create<UpdateCurrencyRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateCurrencyRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.UpdateCurrencyAsync(currency.Code, request);

        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateCurrencyRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Cannot_Create_Currency_With_Code_Exists()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new CurrencyController(mediatorMock);

        var request = new Fixture().Create<CreateCurrencyRequest>();

        _ = mediatorMock
            .Send(Arg.Any<CreateCurrencyRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Valid = false,
                Message = Currency.CodeExisted
            });

        var response = await controller.CreateCurrencyAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Currency.CodeExisted);

        _ = mediatorMock.Received(1).Send(Arg.Any<CreateCurrencyRequest>(), Arg.Any<CancellationToken>());
    }

    [SetUp]
    public override void Setup() => base.Setup();

    private static Fixture CreateCurrencyModelFixture()
    {
        var fixture = new Fixture();
        _ = fixture.Create<CurrencyModel>();
        return fixture;
    }
}

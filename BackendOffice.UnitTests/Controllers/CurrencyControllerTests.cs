using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Currency;
using TGS.Services.BackOffice.Application.Resources;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class CurrencyControllerTests : BaseTests
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new CurrencyController(
            _mediatorMock.Object);
        _controller.ControllerContext.HttpContext = HttpContextMock.Object;
    }

    private CurrencyController _controller=default!;
    private readonly Mock<IMediator> _mediatorMock = new();

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Currencies(bool showAll)
    {
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
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };
        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.UpdatedOn);

        var expectedRoles = showAll ? orderedRoles.ToArray() : orderedRoles.Skip(skip).Take(take).ToArray();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCurrenciesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<CurrencyModel>
            {
                Valid = true,
                Items = expectedRoles
            });

        var response = await _controller.GetCurrenciesAsync(request);
        response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            response.Items[i].Code.Should().Be(expectedRoles[i].Code);
            response.Items[i].Name.Should().Be(expectedRoles[i].Name);
        }

        _mediatorMock.Verify(x => x.Send(It.IsAny<GetCurrenciesRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Cannot_Create_Currency_With_Code_Exists()
    {
        var list = CreateCurrencyModelFixture()
            .CreateMany<CurrencyModel>(99)
            .ToArray();

        var request = new Fixture().Create<CreateCurrencyRequest>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateCurrencyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse {Valid = false, Message = Currency.CodeExisted});

        var response = await _controller.CreateCurrencyAsync(request);
        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Currency.CodeExisted);

        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateCurrencyRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Create_Currency()
    {
        var request = new Fixture().Create<CreateCurrencyRequest>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateCurrencyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse {Valid = true});

        var response = await _controller.CreateCurrencyAsync(request);
        response.Valid.Should().BeTrue();

        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateCurrencyRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Update_Currency()
    {
        var currency = CreateCurrencyModelFixture().Create<CurrencyModel>();
        var request = new Fixture().Create<UpdateCurrencyRequest>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateCurrencyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse {Valid = true});

        var response = await _controller.UpdateCurrencyAsync(currency.Code, request);

        response.Valid.Should().BeTrue();
        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateCurrencyRequest>(), It.IsAny<CancellationToken>()));
    }

    private Fixture CreateCurrencyModelFixture()
    {
        var fixture = new Fixture();
        fixture.Create<CurrencyModel>();
        return fixture;
    }
}
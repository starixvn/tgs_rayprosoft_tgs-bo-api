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
using TGS.Services.BackOffice.Application.Models.Language;
using TGS.Services.BackOffice.Application.Resources;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class LanguageControllerTests : BaseTests
{
    private readonly Mock<IMediator> _mediatorMock = new();

    private LanguageController _controller = default!;

    [Test]
    public async Task Can_Create_Language()
    {
        var request = new Fixture().Create<CreateLanguageRequest>();

        _ = _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateLanguageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = true });

        var response = await _controller.CreateLanguageAsync(request);
        _ = response.Valid.Should().BeTrue();

        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateLanguageRequest>(), It.IsAny<CancellationToken>()));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Languages(bool showAll)
    {
        var list = CreateLanguageModelFixture()
            .CreateMany<LanguageModel>(99)
            .ToArray();

        var request = new GetLanguagesRequest
        {
            CultureCode = "CultureCode",
            LanguageCode = "LanguageCode",
            Page = 1,
            Rows = 10,
            SortBy = "updatedOn",
            OrderBy = SortOrder.Descending,
            ShowAll = showAll
        };
        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.UpdatedOn);

        var expectedRoles = showAll ? orderedRoles.ToArray() : orderedRoles.Skip(skip).Take(take).ToArray();

        _ = _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLanguagesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<LanguageModel>
            {
                Valid = true,
                Items = expectedRoles
            });

        var response = await _controller.GetLanguagesAsync(request);
        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].CultureCode.Should().Be(expectedRoles[i].CultureCode);
            _ = response.Items[i].LanguageCode.Should().Be(expectedRoles[i].LanguageCode);
            _ = response.Items[i].Name.Should().Be(expectedRoles[i].Name);
        }

        _mediatorMock.Verify(x => x.Send(It.IsAny<GetLanguagesRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Update_Language()
    {
        var language = CreateLanguageModelFixture().Create<LanguageModel>();
        var request = new Fixture().Create<UpdateLanguageRequest>();

        _ = _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateLanguageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = true });

        var response = await _controller.UpdateLanguageAsync(language.CultureCode, request);

        _ = response.Valid.Should().BeTrue();
        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateLanguageRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Cannot_Create_Language_With_Culture_Code_Exists()
    {
        var list = CreateLanguageModelFixture()
            .CreateMany<LanguageModel>(99)
            .ToArray();

        var request = new Fixture().Create<CreateLanguageRequest>();

        _ = _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateLanguageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = false, Message = Language.CultureCodeExisted });

        var response = await _controller.CreateLanguageAsync(request);
        _ = response.Valid.Should().BeFalse();
        _ = response.Message.Should().Be(Language.CultureCodeExisted);

        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateLanguageRequest>(), It.IsAny<CancellationToken>()));
    }

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new LanguageController(
            _mediatorMock.Object);
    }

    private static Fixture CreateLanguageModelFixture()
    {
        var fixture = new Fixture();
        _ = fixture.Create<LanguageModel>();
        return fixture;
    }
}

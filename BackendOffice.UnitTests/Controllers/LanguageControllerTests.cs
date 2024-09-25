using AutoFixture;
using FluentAssertions;
using MediatR;
using TGS.Apis.BackendOffice.Controllers;
using TGS.Services.BackOffice.Application.Models.Language;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class LanguageControllerTests : BaseTests
{
    [Test]
    public async Task Can_Create_Language()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new LanguageController(mediatorMock);

        var request = new Fixture().Create<CreateLanguageRequest>();

        _ = mediatorMock
            .Send(Arg.Any<CreateLanguageRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.CreateLanguageAsync(request);
        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<CreateLanguageRequest>(), Arg.Any<CancellationToken>());
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Languages(bool showAll)
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new LanguageController(mediatorMock);

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
            OrderBy = Core.Models.Messages.SortOrder.Descending,
            ShowAll = showAll
        };
        var (skip, take) = GetPage(request);
        var orderedRoles = list.OrderByDescending(p => p.UpdatedOn);

        var expectedRoles = showAll ? [.. orderedRoles] : orderedRoles.Skip(skip).Take(take).ToArray();

        _ = mediatorMock
            .Send(Arg.Any<GetLanguagesRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<LanguageModel>
            {
                Valid = true,
                Items = expectedRoles
            });

        var response = await controller.GetLanguagesAsync(request);

        _ = response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            _ = response.Items[i].CultureCode.Should().Be(expectedRoles[i].CultureCode);
            _ = response.Items[i].LanguageCode.Should().Be(expectedRoles[i].LanguageCode);
            _ = response.Items[i].Name.Should().Be(expectedRoles[i].Name);
        }

        _ = mediatorMock.Received(1).Send(Arg.Any<GetLanguagesRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Can_Update_Language()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new LanguageController(mediatorMock);

        var language = CreateLanguageModelFixture().Create<LanguageModel>();

        var request = new Fixture().Create<UpdateLanguageRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateLanguageRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.UpdateLanguageAsync(language.CultureCode, request);

        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateLanguageRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Cannot_Create_Language_With_Culture_Code_Exists()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new LanguageController(mediatorMock);

        var request = new Fixture().Create<CreateLanguageRequest>();

        _ = mediatorMock
            .Send(Arg.Any<CreateLanguageRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                Valid = false,
                Message = Language.CultureCodeExisted
            });

        var response = await controller.CreateLanguageAsync(request);

        _ = response.Valid.Should().BeFalse();

        _ = response.Message.Should().Be(Language.CultureCodeExisted);

        _ = mediatorMock.Send(Arg.Any<CreateLanguageRequest>(), Arg.Any<CancellationToken>());
    }

    [SetUp]
    public override void Setup() => base.Setup();

    private static Fixture CreateLanguageModelFixture()
    {
        var fixture = new Fixture();
        _ = fixture.Create<LanguageModel>();
        return fixture;
    }
}

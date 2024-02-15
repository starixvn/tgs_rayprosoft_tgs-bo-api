using AutoFixture;
using FluentAssertions;
using MediatR;
using NSubstitute;
using TGS.Apis.BackendOffice.Controllers.Brand;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class BrandControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Update_Brand()
    {
        var mediatorMock = Substitute.For<IMediator>();

        var controller = new BrandController(mediatorMock);

        var brand = CreateBrandModelFixture().Create<BrandModel>();

        var request = new Fixture().Create<UpdateBrandRequest>();

        _ = mediatorMock
            .Send(Arg.Any<UpdateBrandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { Valid = true });

        var response = await controller.UpdateBrandAsync(brand.Id, request);

        _ = response.Valid.Should().BeTrue();

        _ = mediatorMock.Received(1).Send(Arg.Any<UpdateBrandRequest>(), Arg.Any<CancellationToken>());
    }

    private static Fixture CreateBrandModelFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<BrandModel>(u => u
            .With(s => s.GameProviders, [])
            .With(s => s.Languages, [])
            .With(s => s.Currencies, []));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}
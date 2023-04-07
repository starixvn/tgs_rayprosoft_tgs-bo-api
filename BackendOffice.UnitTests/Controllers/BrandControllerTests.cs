using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
using TGS.Apis.BackendOffice.Controllers.Brand;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Resources;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class BrandControllerTests : BaseTests
{
    private readonly Mock<IMediator> _mediatorMock = new();

    private BrandController _controller = default!;

    [Test]
    public async Task Can_Update_Brand()
    {
        var brand = CreateBrandModelFixture().Create<BrandModel>();
        var request = new Fixture().Create<UpdateBrandRequest>();

        _ = _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateBrandRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = true });

        var response = await _controller.UpdateBrandAsync(brand.Id, request);

        _ = response.Valid.Should().BeTrue();
        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateBrandRequest>(), It.IsAny<CancellationToken>()));
    }
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new BrandController(
            _mediatorMock.Object);
    }

    private static Fixture CreateBrandModelFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<BrandModel>(u => u
            .With(s => s.GameProviders, System.Array.Empty<BrandGameProviderModel>())
            .With(s => s.Languages, System.Array.Empty<BrandLanguageModel>())
            .With(s => s.Currencies, System.Array.Empty<BrandCurrencyModel>()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}

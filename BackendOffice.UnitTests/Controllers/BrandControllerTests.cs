using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using TGS.Apis.BackendOffice.Controllers.Brand;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Resources;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class BrandControllerTests : BaseTests
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _controller = new BrandController(
            _mediatorMock.Object);
    }

    private BrandController _controller = default!;
    private readonly Mock<IMediator> _mediatorMock = new();

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get_Brands(bool showAll)
    {
        var list = CreateBrandModelFixture()
            .CreateMany<BrandModel>(99)
            .ToArray();

        var request = new GetBrandsRequest
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
            .Setup(m => m.Send(It.IsAny<GetBrandsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<BrandModel>
            {
                Valid = true,
                Items = expectedRoles
            });

        var response = await _controller.GetBrandsAsync(request);
        response.Valid.Should().BeTrue();

        for (var i = 0; i < response.Items.Length; i++)
        {
            response.Items[i].Id.Should().Be(expectedRoles[i].Id);
            response.Items[i].Name.Should().Be(expectedRoles[i].Name);
        }

        _mediatorMock.Verify(x => x.Send(It.IsAny<GetBrandsRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Cannot_Create_Brand_With_Name_Exists()
    {
        var list = CreateBrandModelFixture()
            .CreateMany<BrandModel>(99)
            .ToArray();

        var request = new Fixture().Create<CreateBrandRequest>();
        request.Name = list.FirstOrDefault()?.Name;

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateBrandRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = false, Message = Brand.CodeExisted });

        var response = await _controller.CreateBrandAsync(request);
        response.Valid.Should().BeFalse();
        response.Message.Should().Be(Brand.CodeExisted);

        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateBrandRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Create_Brand()
    {
        var request = new Fixture().Create<CreateBrandRequest>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateBrandRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = true });

        var response = await _controller.CreateBrandAsync(request);
        response.Valid.Should().BeTrue();

        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateBrandRequest>(), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Update_Brand()
    {
        var brand = CreateBrandModelFixture().Create<BrandModel>();
        var request = new Fixture().Create<UpdateBrandRequest>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateBrandRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MessageResponse { Valid = true });

        var response = await _controller.UpdateBrandAsync(brand.Id, request);

        response.Valid.Should().BeTrue();
        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateBrandRequest>(), It.IsAny<CancellationToken>()));
    }

    private Fixture CreateBrandModelFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<BrandModel>(u => u
            .With(s => s.GameProviders, new BrandGameProviderModel[] { })
            .With(s => s.Languages, new BrandLanguageModel[] { })
            .With(s => s.Currencies, new BrandCurrencyModel[] { }));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}
using AutoFixture;
using FluentAssertions;
using NSubstitute;
using Tgs.Agent.Dividend.Client;
using Tgs.Agent.Dividend.Client.Models;
using TGS.Apis.BackendOffice.Controllers.Brand;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandContract;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class BrandContractControllerTests : BaseTests
{
    [SetUp]
    public override void Setup() => base.Setup();

    [Test]
    public async Task Can_Create_Brand_Contract()
    {
        var fakeBrandContractClient = Substitute.For<IBrandContractClient>();

        var controller = new BrandContractController { ControllerContext = { HttpContext = HttpContextMock } };

        var request = new Fixture().Create<CreateBrandContractRequest>();

        var response = await controller.CreateAsync(fakeBrandContractClient, request);

        _ = response.Valid.Should().BeTrue();

        _ = fakeBrandContractClient.ReceivedWithAnyArgs(1).CreateBrandContractAsync(null!, default);
    }

    [Test]
    public async Task Can_Get_Brand_Contracts()
    {
        var fakeBrandContractClient = Substitute.For<IBrandContractClient>();

        var controller = new BrandContractController { ControllerContext = { HttpContext = HttpContextMock } };

        var brandContracts = new Fixture().CreateMany<BrandContract>(100);

        _ = fakeBrandContractClient.GetBrandContractAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(brandContracts.ToAsyncEnumerable());

        var request = new GetBrandContractsRequest
        {
            BrandId = Guid.NewGuid(),
            Page = 1,
            Rows = 15,
            ShowAll = false,
            SortBy = "name"
        };

        var response = await controller.GetBrandContractsAsync(fakeBrandContractClient, request);

        Assert.That(response.Valid, Is.True);
        Assert.That(response.TotalCount, Is.EqualTo(100));
        Assert.That(response.Items.Length, Is.EqualTo(15));

        _ = fakeBrandContractClient.ReceivedWithAnyArgs(1).GetBrandContractAsync(default, default);
    }
}
using FluentAssertions;
using Moq;
using Tgs.BrandCertificate.Clients;
using Tgs.Promotion.Clients;
using TGS.Apis.BackendOffice.Controllers.Messages.Promotion;
using TGS.Apis.BackendOffice.Controllers.Promotion;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class PromotionControllerTests : BaseTests
{
    [Test]
    public async Task Can_Create_Promotion_Time_Period_With_Minus_420_Time_Span_Minutes()
    {
        base.Setup();

        var controller = new PromotionController();
        var brandServiceClientMock = new Mock<IBrandServiceClient>();
        var promotionServiceClientMock = new Mock<IPromotionServiceClient>();

        controller.ControllerContext.HttpContext = HttpContextMock.Object;

        var brandId = Guid.NewGuid();

        var request = new CreatePromotionRequest
        {
            BrandId = brandId,
            From = DateOnly.Parse("2022/01/20"),
            To = DateOnly.Parse("2022/03/20"),
            PromotionType = "ReliefMoneyDaily"
        };

        var brandInfo = new BrandInfo
        {
            Id = brandId,
            TimeSpanMinutes = -420
        };

        _ = brandServiceClientMock.Setup(x => x.GetBrandInfoAsync(It.IsAny<Guid>(), default)).ReturnsAsync(brandInfo);

        var response = await controller.CreatePromotionAsync(promotionServiceClientMock.Object, brandServiceClientMock.Object, request);
        _ = response.Valid.Should().BeTrue();

        var from = new DateTimeOffset(2022, 1, 20, 0, 0, 0, new(-7, 0, 0));
        var to = new DateTimeOffset(2022, 3, 20, 0, 0, 0, new(-7, 0, 0));

        promotionServiceClientMock.Verify(s => s.CreatePromotionAsync(
            It.IsAny<Guid>(),
            It.Is<CreatePromotionMainInfo>(x => x.EventDuring == new During(from.UtcDateTime, to.UtcDateTime)),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Check_Promotion_Time_Period_With_Plus_420_Time_Span_Minutes()
    {
        base.Setup();

        var controller = new PromotionController();
        var brandServiceClientMock = new Mock<IBrandServiceClient>();
        var promotionServiceClientMock = new Mock<IPromotionServiceClient>();

        controller.ControllerContext.HttpContext = HttpContextMock.Object;

        var brandId = Guid.NewGuid();

        var request = new CreatePromotionRequest
        {
            BrandId = brandId,
            From = DateOnly.Parse("2022/01/20"),
            To = DateOnly.Parse("2022/03/20"),
            PromotionType = "ReliefMoneyDaily"
        };

        var brandInfo = new BrandInfo
        {
            Id = brandId,
            TimeSpanMinutes = 420
        };

        _ = brandServiceClientMock.Setup(x => x.GetBrandInfoAsync(It.IsAny<Guid>(), default)).ReturnsAsync(brandInfo);

        var response = await controller.CreatePromotionAsync(promotionServiceClientMock.Object, brandServiceClientMock.Object, request);
        _ = response.Valid.Should().BeTrue();

        var from = new DateTimeOffset(2022, 1, 20, 0, 0, 0, new(7, 0, 0));
        var to = new DateTimeOffset(2022, 3, 20, 0, 0, 0, new(7, 0, 0));

        promotionServiceClientMock.Verify(s => s.CreatePromotionAsync(
            It.IsAny<Guid>(),
            It.Is<CreatePromotionMainInfo>(x => x.EventDuring == new During(from.UtcDateTime, to.UtcDateTime)),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()));
    }
}

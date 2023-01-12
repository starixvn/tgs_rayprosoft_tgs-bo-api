using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.Promotion;
using TGS.Apis.BackendOffice.Controllers.Promotion;
using TGS.Services.BackOffice.Application.Queries.BrandHandlers;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class PromotionControllerTests : BaseTests
{
    [Test]
    public async Task Check_Promotion_Time_Period_1()
    {
        base.Setup();

        var controller = new PromotionController();
        var brandTimeSpanMinutesMock = new Mock<IGetService<GetBrandTimeSpanMinutesQuery, int>>();
        var promotionServiceClientMock = new Mock<IPromotionServiceClient>();

        controller.ControllerContext.HttpContext = HttpContextMock.Object;

        var brandId = Guid.NewGuid();

        var request = new CreatePromotionRequest
        {
            BrandId = brandId,
            From = "2022/01/20",
            To = "2022/03/20",
            PromotionType = "ReliefMoneyDaily"
        };

        _ = brandTimeSpanMinutesMock.Setup(x => x.GetAsync(It.IsAny<GetBrandTimeSpanMinutesQuery>(), default)).ReturnsAsync(420);

        var response = await controller.CreatePromotionAsync(promotionServiceClientMock.Object, brandTimeSpanMinutesMock.Object, request);
        _ = response.Valid.Should().BeTrue();

        var from = new DateTimeOffset(2022, 1, 20, 0, 0, 0, new(7, 0, 0));
        var to = new DateTimeOffset(2022, 3, 20, 0, 0, 0, new(7, 0, 0));

        promotionServiceClientMock.Verify(s => s.CreatePromotionAsync(
            It.IsAny<Guid>(),
            It.Is<CreatePromotionMainInfo>(x => x.EventDuring == new During(from.UtcDateTime, to.UtcDateTime)),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Create_Promotion_Time_Period_2()
    {
        base.Setup();

        var controller = new PromotionController();
        var brandTimeSpanMinutesMock = new Mock<IGetService<GetBrandTimeSpanMinutesQuery, int>>();
        var promotionServiceClientMock = new Mock<IPromotionServiceClient>();

        controller.ControllerContext.HttpContext = HttpContextMock.Object;

        var brandId = Guid.NewGuid();

        var request = new CreatePromotionRequest
        {
            BrandId = brandId,
            From = "2022/01/20",
            To = "2022/03/20",
            PromotionType = "ReliefMoneyDaily"
        };

        _ = brandTimeSpanMinutesMock.Setup(x => x.GetAsync(It.IsAny<GetBrandTimeSpanMinutesQuery>(), default)).ReturnsAsync(-420);

        var response = await controller.CreatePromotionAsync(promotionServiceClientMock.Object, brandTimeSpanMinutesMock.Object, request);
        _ = response.Valid.Should().BeTrue();

        var from = new DateTimeOffset(2022,1,20,0,0,0,new (-7,0,0));
        var to = new DateTimeOffset(2022, 3, 20, 0, 0, 0, new(-7, 0, 0));

        promotionServiceClientMock.Verify(s => s.CreatePromotionAsync(
            It.IsAny<Guid>(),
            It.Is<CreatePromotionMainInfo>(x => x.EventDuring == new During(from.UtcDateTime, to.UtcDateTime)),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()));
    }

}

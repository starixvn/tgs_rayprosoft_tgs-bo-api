using FluentAssertions;
using Moq;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.Promotion;
using TGS.Apis.BackendOffice.Controllers.Promotion;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.Promotion;
using PromotionStatus = Tgs.Promotion.Clients.PromotionStatus;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class PromotionControllerTests : BaseTests
{
    [Test]
    public async Task Can_Create_Promotion()
    {
        base.Setup();

        var controller = new PromotionController();

        var createMainInfoServiceMock = new Mock<IGetService<CreatePromotionMainInfoCommand, Guid>>();
        var updateContentServiceMock = new Mock<ICommandService<UpdatePromotionContentCommand>>();
        var updateRuleServiceMock = new Mock<ICommandService<UpdatePromotionRuleCommand>>();
        var updateStatusServiceMock = new Mock<ICommandService<UpdatePromotionStatusCommand>>();
        
        controller.ControllerContext.HttpContext = HttpContextMock.Object;

        var brandId = Guid.NewGuid();

        var request = new CreatePromotionRequest
        {
            BrandId = brandId,
            From = DateOnly.Parse("2022/01/20"),
            To = DateOnly.Parse("2022/03/20"),
            PromotionType = "ReliefMoneyDaily",
            Status = SwitchStatus.Enabled
        };
        
        var promotionId = Guid.NewGuid();
        
        _ = createMainInfoServiceMock
            .Setup(x => x.GetAsync(
                It.Is<CreatePromotionMainInfoCommand>(info => 
                    info.PromotionType == request.PromotionType
                    && info.BrandId == brandId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(promotionId);
        
        var response = await controller.CreatePromotionAsync(
            createMainInfoServiceMock.Object,
            updateContentServiceMock.Object,
            updateRuleServiceMock.Object,
            updateStatusServiceMock.Object,
            request);

        _ = response.Valid.Should().BeTrue();

        updateContentServiceMock.Verify(s => s.ExecuteAsync(
            It.Is<UpdatePromotionContentCommand>(x => x.PromotionId == promotionId),
            It.IsAny<CancellationToken>()));

        updateRuleServiceMock.Verify(s => s.ExecuteAsync(
            It.Is<UpdatePromotionRuleCommand>(x => x.PromotionId == promotionId),
            It.IsAny<CancellationToken>()));

        updateStatusServiceMock.Verify(s => s.ExecuteAsync(
            It.Is<UpdatePromotionStatusCommand>(x => 
                x.PromotionId == promotionId && x.Status == PromotionStatus.Enabled),
            It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Can_Update_Promotion()
    {

        base.Setup();

        var controller = new PromotionController();
    
        var updateMainInfoServiceMock = new Mock<ICommandService<UpdatePromotionMainInfoCommand>>();
        var updateContentServiceMock = new Mock<ICommandService<UpdatePromotionContentCommand>>();
        var updateRuleServiceMock = new Mock<ICommandService<UpdatePromotionRuleCommand>>();
        var updateStatusServiceMock = new Mock<ICommandService<UpdatePromotionStatusCommand>>();

        controller.ControllerContext.HttpContext = HttpContextMock.Object;

        var brandId = Guid.NewGuid();

        var request = new UpdatePromotionRequest
        {
            From = DateOnly.Parse("2022/01/20"),
            To = DateOnly.Parse("2022/03/20"),
            PromotionType = "ReliefMoneyDaily",
            Status = SwitchStatus.Enabled
        };

        var promotionId = Guid.NewGuid();

        var response = await controller.UpdatePromotionAsync(
            updateMainInfoServiceMock.Object,
            updateContentServiceMock.Object,
            updateRuleServiceMock.Object,
            updateStatusServiceMock.Object,
            promotionId,
            request);

        _ = response.Valid.Should().BeTrue();

        updateMainInfoServiceMock.Verify(x => x.ExecuteAsync(
            It.Is<UpdatePromotionMainInfoCommand>(info =>
                info.PromotionType == request.PromotionType
                && info.PromotionId == promotionId),
            It.IsAny<CancellationToken>()));

        updateContentServiceMock.Verify(s => s.ExecuteAsync(
            It.Is<UpdatePromotionContentCommand>(x => x.PromotionId == promotionId),
            It.IsAny<CancellationToken>()));

        updateRuleServiceMock.Verify(s => s.ExecuteAsync(
            It.Is<UpdatePromotionRuleCommand>(x => x.PromotionId == promotionId),
            It.IsAny<CancellationToken>()));

        updateStatusServiceMock.Verify(s => s.ExecuteAsync(
            It.Is<UpdatePromotionStatusCommand>(x => 
                x.PromotionId == promotionId && x.Status == PromotionStatus.Enabled),
            It.IsAny<CancellationToken>()));
    }
}

using NSubstitute;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Clients.Models;
using Tgs.Promotion.Clients.Models.PromotionDispatch;
using TGS.Apis.BackendOffice.Controllers.Messages.PromotionDispatch;
using TGS.Apis.BackendOffice.Controllers.Promotion;

namespace TGS.Tests.BackendOffice.UnitTests.Controllers;

[Parallelizable]
public class PromotionDispatchControllerTests : BaseTests
{
    [Test]
    public async Task Get_Promotion_Dispatches()
    {
        var fakePromotionDispatchClient = Substitute.For<IPromotionDispatchClient>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

        var sut = new PromotionDispatchController(fakePromotionDispatchClient, fakePromotionServiceClient)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        _ = fakePromotionDispatchClient.GetPromotionDispatchesAsync(Arg.Any<GetPromotionDispatchesRequest>())
            .Returns(new PagedResponse<PromotionDispatchModel>
            {
                Valid = true
            });
        var request = new GetPromotionDispatchesViewRequest();

        _ = await sut.GetPromotionDispatchesAsync(request);

        _ = await fakePromotionDispatchClient
            .ReceivedWithAnyArgs(1)
            .GetPromotionDispatchesAsync(null!);
    }

    [Test]
    public async Task Dispatch_Promotions()
    {
        var fakePromotionDispatchClient = Substitute.For<IPromotionDispatchClient>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

        var sut = new PromotionDispatchController(fakePromotionDispatchClient, fakePromotionServiceClient)
        {
            ControllerContext = { HttpContext = HttpContextMock }
        };

        _ = fakePromotionDispatchClient.DispatchPromotionAsync(Arg.Any<DispatchPromotionBonusRequest>())
            .Returns(new MessageResponse()
            {
                Valid = true
            });

        var request = new DispatchPromotionBonusViewRequest
        {
            Players =
            [
                new() {
                    PlayerCode = "playerCode",
                    PromotionBonusId = Guid.NewGuid()
                }
            ]
        };

        _ = await sut.DispatchPromotionsAsync(request);

        _ = await fakePromotionDispatchClient
            .ReceivedWithAnyArgs(1)
            .DispatchPromotionAsync(null!);

        await fakePromotionServiceClient
            .ReceivedWithAnyArgs(1)
            .DistributePromotionBonusManuallyAsync(null!);
    }

    [SetUp]
    public override void Setup() => base.Setup();
}
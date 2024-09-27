using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Promotion.Clients;
using TGS.Apis.BackendOffice.Controllers.Messages.PromotionDispatch;
using Tgs.Promotion.Clients.Models;
using Tgs.Promotion.Clients.Models.PromotionDispatch;
using MessageResponse = TGS.Core.Models.Messages.MessageResponse;
using TGS.Apis.BackendOffice.Attributes;

namespace TGS.Apis.BackendOffice.Controllers.Promotion;

[Route("api/[controller]")]
[ApiController]
public class PromotionDispatchController(IPromotionDispatchClient client, IPromotionServiceClient promotionServiceClient) : BaseContextController
{
    [HttpPost(Name = "DispatchPromotions")]
    [BOAuthorize(["Promotion.PayoutList.Update"])]
    [Description("Dispatch promotion")]
    public async Task<MessageResponse> DispatchPromotionsAsync(
        [FromBody] DispatchPromotionBonusViewRequest request)
    {
        request.OperatorUserId = Guid.Parse(UserId);

        var distributeRequest = new DistributePromotionBonusRequest
        {
            Distributor = Username,
            Players = request.Players.Select(x => new DistributePlayer
            {
                PromotionBonusId = x.PromotionBonusId,
                PlayerCode = x.PlayerCode
            }).ToArray()
        };

        await promotionServiceClient.DistributePromotionBonusManuallyAsync(distributeRequest);

        var dispatchRequest = new DispatchPromotionBonusRequest
        {
            OperatorUserId = Guid.Parse(UserId),
            Players = request.Players.Select(x => new DispatchPlayer
            {
                PromotionBonusId = x.PromotionBonusId,
                PlayerCode = x.PlayerCode
            }).ToArray()
        };

        var response = await client.DispatchPromotionAsync(dispatchRequest);

        return new MessageResponse { Valid = response.Valid, Message = response.Message };
    }

    [HttpGet(Name = "GetPromotionDispatches")]
    [BOAuthorize(["Promotion.PayoutList"])]
    [Description("Get promotion dispatches")]
    public async Task<Core.Models.Messages.PagedResponse<PromotionDispatchViewModel>> GetPromotionDispatchesAsync(
        [FromQuery] GetPromotionDispatchesViewRequest request)
    {
        var promotionRequest = new GetPromotionDispatchesRequest
        {
            BrandId = request.BrandId,
            PromotionGroup = request.PromotionGroup,
            PromotionName = request.PromotionName,
            Username = request.Username,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await client.GetPromotionDispatchesAsync(promotionRequest);

        return new Core.Models.Messages.PagedResponse<PromotionDispatchViewModel>
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items.Select(x => new PromotionDispatchViewModel
            {
                PlayerCode = x.PlayerCode,
                Username = x.Username,
                PromotionId = x.PromotionId,
                PromotionBonusId = x.PromotionBonusId,
                PromotionName = x.PromotionName,
                PromotionGroup = x.PromotionGroup,
                PromotionType = x.PromotionType,
                BonusAmount = x.BonusAmount,
                CreatedOn = x.CreatedOn,
                SentOn = x.SentOn
            }).ToArray()
        };
    }
}
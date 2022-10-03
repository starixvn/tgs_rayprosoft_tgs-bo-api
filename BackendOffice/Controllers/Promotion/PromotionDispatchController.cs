using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.Controllers.Messages.PromotionDispatch;
using TGS.Core.Models.Messages;
using Tgs.Promotion.Clients;

namespace TGS.Apis.BackendOffice.Controllers.Promotion;

[Route("api/[controller]")]
[ApiController]
public class PromotionDispatchController : BaseContextController
{
    private readonly IPromotionDispatchClient _client;

    public PromotionDispatchController(IPromotionDispatchClient client)
    {
        _client = client;
    }

    [HttpGet(Name = "GetPromotionDispatches")]
    [Description("Get promotion dispatches")]
    public async Task<PagedResponse<PromotionDispatchModel>> GetPromotionDispatchesAsync(
        [FromQuery] GetPromotionDispatchesRequest request)
    {
        var promotionRequest = new Tgs.Promotion.Clients.Models.PromotionDispatch.GetPromotionDispatchesRequest
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

        var response = await _client.GetPromotionDispatchesAsync(promotionRequest);

        return new PagedResponse<PromotionDispatchModel>
        {
            Valid = response.Valid, 
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items.Select(x => new PromotionDispatchModel
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

    [HttpPost(Name = "DispatchPromotions")]
    [Description("Dispatch promotion")]
    public async Task<MessageResponse> DispatchPromotionsAsync(
        [FromBody] DispatchPromotionBonusRequest request)
    {
        request.OperatorUserId = Guid.Parse(UserId);

        var promotionRequest = new Tgs.Promotion.Clients.Models.PromotionDispatch.DispatchPromotionBonusRequest
        {
            OperatorUserId = Guid.Parse(UserId),
            Players = request.Players.Select(x => new Tgs.Promotion.Clients.Models.PromotionDispatch.DispatchPlayer
            {
                PromotionBonusId = x.PromotionBonusId,
                PlayerCode = x.PlayerCode
            }).ToArray()
        };

        var response = await _client.DispatchPromotionAsync(promotionRequest);

        return new MessageResponse { Valid = response.Valid, Message = response.Message };
    }
}
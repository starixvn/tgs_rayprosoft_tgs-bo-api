using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JsonCons.Utilities;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandCertificate.Clients;
using Tgs.Promotion;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.Promotion;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices.Promotion;
using TGS.Services.BackOffice.Domain;
using PromotionStatus = TGS.Core.Models.Enums.PromotionStatus;

namespace TGS.Apis.BackendOffice.Controllers.Promotion;

[Route("api/[controller]")]
[ApiController]
public class PromotionController : BaseContextController
{
    private readonly ImmutableArray<string> _promotionTypes = new string[]
    {
        "ReliefMoneyDaily",
        "ReliefMoneyWeekly",
        "ReliefMoneyMonthly",
        "TurnoverRebate",
        "TurnoverRebateWeekly"
    }.ToImmutableArray();

    [HttpPut("ChangeOrders", Name = "ChangeOrder")]
    [Description("Change Promotion Display Order")]
    public async Task<MessageResponse> ChangeOrderAsync(
        [FromServices] IPromotionServiceClient promotionServiceClient,
        ChangeOrderRequest request)
    {
        var promotionOrders = request.Orders
            .Select(x => new PromotionOrder(x.Id, x.DisplayOrder))
            .ToArray();

        await promotionServiceClient.ChangePromotionOrdersAsync(
                promotionOrders,
                Username,
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse { Valid = true };
    }

    [HttpPost(Name = "CreatePromotion")]
    [Description("Create a promotion")]
    public async Task<MessageResponse> CreatePromotionAsync(
        [FromServices] IPromotionServiceClient promotionServiceClient,
        [FromServices] IBrandServiceClient brandServiceClient,
        [FromBody] CreatePromotionRequest request)
    {
        var from = new DateTimeOffset(request.From.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8));
        var to = new DateTimeOffset(request.To.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8));

        if (_promotionTypes.Contains(request.PromotionType))
        {
            var brandInfo = await brandServiceClient.GetBrandInfoAsync(request.BrandId)
                .ConfigureAwait(false);

            var timeSpanMinutes = (int)brandInfo?.TimeSpanMinutes!;

            from = new DateTimeOffset(request.From.ToDateTime(TimeOnly.MinValue), TimeSpan.FromMinutes(timeSpanMinutes));
            to = new DateTimeOffset(request.To.ToDateTime(TimeOnly.MinValue), TimeSpan.FromMinutes(timeSpanMinutes));
        }

        var createInfo = new CreatePromotionMainInfo
        {
            Name = request.Name,
            EventDuring = (from.UtcDateTime, to.UtcDateTime),
            PromotionType = request.PromotionType,
            IsAutoSent = request.AutoSentPlayerBonus,
            PromotionLimitAmount = request.Unlimited ? null : request.TotalAmountLimit
        };

        var promotionId = await promotionServiceClient.CreatePromotionAsync(
            request.BrandId,
            createInfo,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        await promotionServiceClient.SetPromotionVipLevelsAsync(
            promotionId,
            request.VipLevels,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        var content = new PromotionContent
        {
            Content = request.Content,
            Convention = request.Terms,
            Localization = request.Localization,
            MobileBanner = request.MobileBanner,
            PersonalComputerBanner = request.DesktopBanner,
            SubSubject = request.SubSubject
        };

        await promotionServiceClient.SetPromotionContentAsync(
            promotionId,
            content,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        var baseRule = JsonSerializer.SerializeToDocument(new
        {
            request.GameProviderIds,
            request.BonusAmountLimit,
            request.Percentage,
            request.TurnoverMultiple,
            request.GameType,
            request.VipLevels
        }, PromotionUtility.SerializerOptions);

        if (request.Rule == null)
        {
            await promotionServiceClient.SetPromotionRuleAsync(
                promotionId,
                baseRule,
                Username,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }
        else
        {
            await promotionServiceClient.SetPromotionRuleAsync(
                promotionId,
                JsonMergePatch.ApplyMergePatch(baseRule.RootElement, request.Rule.RootElement),
                Username,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }

        await promotionServiceClient.SetPromotionStatusAsync(
            promotionId,
            request.Status switch
            {
                SwitchStatus.Enabled => Tgs.Promotion.Clients.PromotionStatus.Enabled,
                SwitchStatus.Disabled => Tgs.Promotion.Clients.PromotionStatus.Disabled,
                _ => throw new NotImplementedException()
            },
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse { Valid = true };
    }

    [HttpGet("rule/{promotionId}", Name = "GetPromotionRule")]
    public async Task<JsonDocument> GetPromotionRuleAsync(
        [FromServices] IGetService<GetPromotionRuleQuery, PromotionRuleInfo> getService,
        Guid promotionId)
    {
        var query = new GetPromotionRuleQuery(promotionId);

        var rule = await getService.GetAsync(query).ConfigureAwait(false);

        return rule?.CompaignDetail;
    }

    [HttpGet(Name = "GetPromotions")]
    [Description("Get promotions")]
    public async Task<PagedResponse<PromotionModel>> GetPromotionsAsync(
        [FromServices] IPromotionClient promotionClient,
        [FromQuery] GetPromotionsRequest request)
    {
        var promotionRequest = new Tgs.Promotion.Clients.Models.Promotion.GetPromotionsRequest
        {
            BrandId = request.BrandId,
            Name = request.Name,
            From = request.From,
            To = request.To,
            Status = (short?)request.Status,
            Type = request.Type,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await promotionClient.GetPromotionsAsync(promotionRequest)
            .ConfigureAwait(false);

        return new PagedResponse<PromotionModel>
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items.Select(x => new PromotionModel
            {
                AmountUsed = x.AmountUsed,
                AutoSentPlayerBonus = x.AutoSentPlayerBonus,
                BonusAmountLimit = x.BonusAmountLimit,
                BrandId = x.BrandId,
                Content = x.Content,
                CreatedByUsername = x.CreatedByUsername,
                CreatedOn = x.CreatedOn,
                DesktopBanner = x.DesktopBanner,
                From = x.From,
                GameProviderIds = x.GameProviderIds,
                GameType = (GameType?)x.GameType,
                Id = x.Id,
                Localization = x.Localization,
                MobileBanner = x.MobileBanner,
                Name = x.Name,
                Percentage = x.Percentage,
                PromotionType = x.PromotionType,
                Status = (SwitchStatus)x.Status,
                SubSubject = x.SubSubject,
                Terms = x.Terms,
                To = x.To,
                TotalAmountLimit = x.TotalAmountLimit,
                TurnoverMultiple = x.TurnoverMultiple,
                Unlimited = x.Unlimited,
                UpdatedByUsername = x.UpdatedByUsername,
                UpdatedOn = x.UpdatedOn,
                VipLevels = x.VipLevels,
                Number = x.Number
            }).ToArray()
        };
    }

    [HttpGet("player", Name = "ListPlayerPromotions")]
    public IAsyncEnumerable<PromotionViewModel> ListPlayerPromotionsAsync(
        [FromServices] IPromotionServiceClient promotionServiceClient,
        [FromQuery] ListPlayerPromotionsRequest request)
    {
        var allowPlayerSelectPromotionTypes = new string[] {
            "FirstDeposit",
            "DailySecondDeposit",
            "DailyFirstDeposit"
        };

        return promotionServiceClient
            .ListPromotionsAsync(request.BrandId, request.PlayerCode, HttpContext.RequestAborted)
            .Where(promotion => allowPlayerSelectPromotionTypes.Contains(promotion.PromotionType))
            .Select(promotion => new PromotionViewModel
            {
                Id = promotion.Id,
                Name = promotion.Name,
                Localization = promotion.Localization
            });
    }

    [HttpPut("{promotionId}", Name = "UpdatePromotion")]
    [Description("Update a promotion")]
    public async Task<MessageResponse> UpdatePromotionAsync(
        [FromServices] IPromotionServiceClient promotionServiceClient,
        [FromServices] IBrandServiceClient brandServiceClient,
        Guid promotionId,
        [FromBody] UpdatePromotionRequest request)
    {
        var from = new DateTimeOffset(request.From.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8));
        var to = new DateTimeOffset(request.To.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8));

        if (_promotionTypes.Contains(request.PromotionType))
        {
            var promotion = await promotionServiceClient.GetPromotionMainInfoAsync(promotionId).ConfigureAwait(false);

            var brandInfo = await brandServiceClient.GetBrandInfoAsync(promotion.BrandId)
                .ConfigureAwait(false);

            var timeSpanMinutes = (int)brandInfo?.TimeSpanMinutes!;

            from = new DateTimeOffset(request.From.ToDateTime(TimeOnly.MinValue), TimeSpan.FromMinutes(timeSpanMinutes));
            to = new DateTimeOffset(request.To.ToDateTime(TimeOnly.MinValue), TimeSpan.FromMinutes(timeSpanMinutes));
        }

        var updateInfo = new CreatePromotionMainInfo
        {
            Name = request.Name,
            EventDuring = (from.UtcDateTime, to.UtcDateTime),
            PromotionType = request.PromotionType,
            IsAutoSent = request.AutoSentPlayerBonus,
            PromotionLimitAmount = request.Unlimited ? null : request.TotalAmountLimit
        };

        await promotionServiceClient.UpdatePromotionMainInfoAsync(
            promotionId,
            updateInfo,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        await promotionServiceClient.SetPromotionVipLevelsAsync(
            promotionId,
            request.VipLevels,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        var content = new PromotionContent
        {
            Content = request.Content,
            Convention = request.Terms,
            Localization = request.Localization,
            MobileBanner = request.MobileBanner,
            PersonalComputerBanner = request.DesktopBanner,
            SubSubject = request.SubSubject
        };
        await promotionServiceClient.SetPromotionContentAsync(
            promotionId,
            content,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        var baseRule = JsonSerializer.SerializeToDocument(new
        {
            request.GameProviderIds,
            request.BonusAmountLimit,
            request.Percentage,
            request.TurnoverMultiple,
            request.GameType,
            request.VipLevels
        }, PromotionUtility.SerializerOptions);

        if (request.Rule == null)
        {
            await promotionServiceClient.SetPromotionRuleAsync(
                promotionId,
                baseRule,
                Username,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }
        else
        {
            await promotionServiceClient.SetPromotionRuleAsync(
                promotionId,
                JsonMergePatch.ApplyMergePatch(baseRule.RootElement, request.Rule.RootElement),
                Username,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }

        await promotionServiceClient.SetPromotionStatusAsync(
            promotionId,
            request.Status switch
            {
                SwitchStatus.Enabled => Tgs.Promotion.Clients.PromotionStatus.Enabled,
                SwitchStatus.Disabled => Tgs.Promotion.Clients.PromotionStatus.Disabled,
                _ => throw new NotImplementedException()
            },
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse { Valid = true };
    }

    [HttpPatch("content/{promotionId}", Name = "UpdatePromotionContent")]
    public async ValueTask<MessageResponse> UpdatePromotionContentAsync(
        [FromServices] ICommandService<UpdatePromotionContentCommand> commandService,
        Guid promotionId,
        UpdatePromotionContentViewModel viewModel)
    {
        var command = new UpdatePromotionContentCommand(
            promotionId,
            viewModel.SubSubject,
            viewModel.Content,
            viewModel.Localization ?? JsonDocument.Parse("{}"),
            viewModel.DesktopBanner,
            viewModel.MobileBanner,
            viewModel.Terms,
            Username);

        await commandService.ExecuteAsync(command, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }

    [HttpPatch("rule/{promotionId}", Name = "UpdatePromotionRule")]
    public async ValueTask<MessageResponse> UpdatePromotionRuleAsync(
        [FromServices] IPromotionServiceClient promotionServiceClient,
        Guid promotionId,
        UpdatePromotionRuleViewModel viewModel)
    {
        var baseRule = JsonSerializer.SerializeToDocument(new
        {
            viewModel.GameProviderIds,
            viewModel.GameType,
            viewModel.VipLevels
        }, PromotionUtility.SerializerOptions);

        if (viewModel.Rule == null)
        {
            await promotionServiceClient.SetPromotionRuleAsync(
                promotionId,
                baseRule,
                Username,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }
        else
        {
            await promotionServiceClient.SetPromotionRuleAsync(
                promotionId,
                JsonMergePatch.ApplyMergePatch(baseRule.RootElement, viewModel.Rule.RootElement),
                Username,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }

        await promotionServiceClient.SetPromotionVipLevelsAsync(
            promotionId,
            viewModel.VipLevels,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }

    [HttpPatch("main/{promotionId}", Name = "UpdatePromotionMain")]
    public async ValueTask<MessageResponse> UpdatePromotionMainInfoAsync(
        [FromServices] IPromotionServiceClient promotionServiceClient,
        [FromServices] IBrandServiceClient brandServiceClient,
        Guid promotionId,
        UpdatePromotionMainInfoViewModel viewModel)
    {
        var from = new DateTimeOffset(viewModel.From.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8));
        var to = new DateTimeOffset(viewModel.To.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8));

        if (_promotionTypes.Contains(viewModel.PromotionType))
        {
            var promotion = await promotionServiceClient.GetPromotionMainInfoAsync(promotionId).ConfigureAwait(false);

            var brandInfo = await brandServiceClient.GetBrandInfoAsync(promotion!.BrandId)
                .ConfigureAwait(false);

            var timeSpanMinutes = (int)brandInfo?.TimeSpanMinutes!;

            from = new DateTimeOffset(viewModel.From.ToDateTime(TimeOnly.MinValue), TimeSpan.FromMinutes(timeSpanMinutes));
            to = new DateTimeOffset(viewModel.To.ToDateTime(TimeOnly.MinValue), TimeSpan.FromMinutes(timeSpanMinutes));
        }

        var updateInfo = new CreatePromotionMainInfo
        {
            Name = viewModel.Name,
            EventDuring = (from.UtcDateTime, to.UtcDateTime),
            PromotionType = viewModel.PromotionType,
            IsAutoSent = viewModel.AutoSentPlayerBonus,
            PromotionLimitAmount = viewModel.Unlimited ? null : viewModel.TotalAmountLimit
        };

        await promotionServiceClient.UpdatePromotionMainInfoAsync(
            promotionId,
            updateInfo,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }

    [HttpPut("{promotionId:guid}/{status}", Name = "UpdatePromotionStatus")]
    [Description("change a Promotion's status")]
    public async Task<MessageResponse> UpdatePromotionStatusAsync(
        [FromServices] IPromotionServiceClient promotionServiceClient,
        Guid promotionId,
        PromotionStatus status)
    {
        await promotionServiceClient.SetPromotionStatusAsync(
                promotionId,
                status switch
                {
                    PromotionStatus.Enabled => Tgs.Promotion.Clients.PromotionStatus.Enabled,
                    PromotionStatus.Disabled => Tgs.Promotion.Clients.PromotionStatus.Disabled,
                    PromotionStatus.Archived => Tgs.Promotion.Clients.PromotionStatus.Archived,
                    _ => throw new NotImplementedException()
                },
                Username,
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new MessageResponse { Valid = true };
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Tgs.Agent.Dividend.Client;
using Tgs.Agent.Dividend.Client.Models;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages.AgentDividend;
using Tgs.BrandCertificate.Clients;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Domain.Enums;
using DistributeStatus = Tgs.Agent.Dividend.Client.Enums.DistributeStatus;

namespace TGS.Apis.BackendOffice.Controllers.Agent;

[Route("api/[controller]")]
[ApiController]
public class AgentDividendController(IAgentDividendClient agentDividendClient, IBrandServiceClient brandServiceClient)
    : BaseContextController
{
    [HttpGet("export/{brandCode}")]
    [Description("Export pre calculate agent dividend")]
    [ByPassAdminLog]
    public async Task<IActionResult> ExportPreCalculateAgentDividendAsync(string brandCode)
    {
        if (UserRoleType != RoleType.Admin)
            return null;

        var brand = await brandServiceClient.GetBrandInfoByCodeAsync(brandCode, HttpContext.RequestAborted);

        if (brand == null)
            return null;

        var agentDividends = await agentDividendClient
            .GetPreCalculateAgentDividend(brand.Id, HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted);

        var exportData = agentDividends.Select(x => new
        {
            x.Id,
            x.ReferencedAgentDividendId,
            x.AgentUsername,
            x.From,
            x.To,
            x.PromotionAmount,
            x.RebateAmount,
            x.DepositAmount,
            x.WithdrawalAmount,
            x.CompanyAdjustAmount,
            x.AgentAdjustAmount,
            x.BonusAdjustAmount,
            x.BonusAmount,
            x.BetAmount,
            x.ValidBetAmount,
            x.OriginalTurnover,
            x.Turnover,
            x.WinLossAmount,
            x.NoIncludeNetProfitAgentAdjustAmount,
            x.NoIncludeNetProfitBonusAdjustAmount,
            x.NoIncludeNetProfitCompanyAdjustAmount,
            x.NetProfit,
            x.LiveDealerWinLoss,
            x.SlotWinLoss,
            x.FishingWinLoss,
            x.ArcadeWinLoss,
            x.TableGameWinLoss,
            x.LotteryWinLoss,
            x.CardGameWinLoss,
            x.SportsWinLoss,
            x.ESportsWinLoss,
            x.VirtualSportsWinLoss,
            x.AgentDividendPercentage,
            x.AgentDividendAmount,
            x.BeforeDividendAmount,
            x.AutoSend,
            AgentDividendMultipleLayerModels = JsonSerializer.Serialize(x.AgentDividendMultipleLayerModels)
        });

        var excel = Export(exportData, "PreCalculateAgentDividend");

        await using var stream = new MemoryStream();

        excel.SaveAs(stream);

        return File(
            new MemoryStream(stream.ToArray()),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "PreCalculateAgentDividend.xlsx");
    }

    [HttpGet]
    [Description("Get agent dividend records")]
    public async Task<PagedResponse<AgentDividendRecordViewModel>> GetAgentDividendRecordsAsync(
        [FromQuery] GetAgentDividendRecordsRequest request)
    {
        var records = await agentDividendClient.GetAgentDividendRecordsAsync(new GetAgentDividendRecords(
                    request.BrandId,
                    (DistributeStatus?)request.Status,
                    request.From,
                    request.To,
                    request.ApprovedFrom,
                    request.ApprovedTo,
                    request.AgentUsername,
                    request.NegativeAgentDividendCarryForward),
                HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted);

        return records.Select(x => new AgentDividendRecordViewModel(
                x.Id,
                (AgentDividendDistributeStatus)x.Status,
                x.From,
                x.To,
                x.AgentUsername,
                x.AgentDividendAmount,
                x.ActualAgentDividendAmount,
                x.NegativeAgentDividendAmount,
                x.NegativeAgentDividendCarryForward,
                x.CreatedOn,
                x.UpdatedOn,
                x.ApprovedOn,
                x.Remark))
            .AsQueryable()
            .CreatePagedResponse(request);
    }

    [HttpPost("send-agent-dividend")]
    [Description("Send the agent dividend")]
    public async Task<MessageResponse> CreateAgentReduceAmountAsync(ManualSendAgentDividendsRequest request)
    {
        await agentDividendClient.SendAgentDividendsAsync(request.AgentDividendRecordIds,
            request.Approve,
            Username,
            request.Remark,
            HttpContext.RequestAborted);

        return CreateValidResponse<MessageResponse>();
    }

    private static XLWorkbook Export<T>(IEnumerable<T> models, string sheetName)
    {
        var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(sheetName);
        var colIdx = 1;

        foreach (var item in typeof(T).GetProperties())
            sheet.Cell(1, colIdx++).Value = item.Name;

        if (models == null)
            return workbook;

        _ = sheet.Cell("A2").InsertData(models);

        return workbook;
    }
}
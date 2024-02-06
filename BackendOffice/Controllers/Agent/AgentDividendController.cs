using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Agent.Dividend.Client;
using Tgs.Agent.Dividend.Client.Models;
using TGS.Apis.BackendOffice.Attributes;
using Tgs.BrandCertificate.Clients;
using Tgs.Core.Infrastructures.Utility;
using TGS.Core.Models.Enums;

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

        if (brand == null) return null;

        var agentDividends = await agentDividendClient
            .GetPreCalculateAgentDividend(brand.Id, HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted);

        foreach (var agentDividendModel in agentDividends)
        {
            agentDividendModel.AgentDividendMultipleLayerModels = new AgentDividendMultipleLayerModel[]
            {
                new AgentDividendMultipleLayerModel
                {
                    ActualDividendPercentage = 1, AgentDividendPercentage = 1, AgentId = Guid.NewGuid()
                }
            };
        }

        var excel = agentDividends.Export("PreCalculateAgentDividend");

        await using var stream = new MemoryStream();

        excel.SaveAs(stream);

        return File(
            new MemoryStream(stream.ToArray()),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "PreCalculateAgentDividend.xlsx");
    }
}
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Agent.Dividend.Client;
using Tgs.Agent.Dividend.Client.Models;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandContract;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public class BrandContractController : BaseContextController
{
    [HttpPost]
    public async ValueTask<MessageResponse> CreateAsync(
        [FromServices] IBrandContractClient brandContractClient,
        CreateBrandContractRequest request)
    {
        var brandId = request.BrandId;

        if (UserRoleType == RoleType.Brand)
            brandId = BrandId!.Value;

        var conditions = request.Conditions.Select(x =>
            new CreateBrandContractCondition(x.Rank,
                x.ActiveNumber,
                x.DividendPercentage,
                x.NetProfit,
                x.WinLossAmount));

        await brandContractClient.CreateBrandContractAsync(new CreateBrandContract(brandId,
                request.Name,
                request.ContractDividendType,
                request.GradeIntervalLimit,
                request.ValidBetAmount,
                request.DepositAmount,
                request.BetDays,
                Username,
                request.GameTypes,
                conditions),
            HttpContext.RequestAborted);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpGet]
    public async Task<PagedResponse<BrandContract>> GetBrandContractsAsync(
        [FromServices] IBrandContractClient brandContractClient,
        [FromQuery] GetBrandContractsRequest request)
    {
        var brandId = request.BrandId;

        if (UserRoleType == RoleType.Brand)
            brandId = BrandId!.Value;

        var list = await brandContractClient.GetBrandContractAsync(brandId, HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted);

        return list.AsQueryable().CreatePagedResponse(request);
    }
}
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

namespace TGS.Apis.BackendOffice.Controllers.BrandGameCredits;

[Route("api/[controller]")]
[ApiController]
public class BrandGameCreditController : BaseContextController
{
    [HttpGet("{brandId:guid}/current", Name = "BrandGameCredit_GetCurrent")]
    public async ValueTask<BrandGameCreditViewModel?> GetAsync(
        [FromServices] IGetService<BrandGameCreditGetCurrentQuery, BrandGameCreditResult> service,
        Guid brandId)
    {
        var result = await service.GetAsync(
            query: new BrandGameCreditGetCurrentQuery(brandId),
            cancellationToken: HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return result is null
            ? null
            : new BrandGameCreditViewModel(
            result.ResellerId,
            result.ResellerCode,
            result.LicenseeId,
            result.LicenseeCode,
            result.BrandId,
            result.BrandCode,
            result.Month,
            result.InitialAmount,
            result.CreditAmount,
            result.DebitAmount,
            result.ProfitAmount,
            result.CreditLimit,
            result.CreditUsed,
            result.CreditUsedPer,
            result.CreditAvailable,
            result.CreditAvailablePer,
            result.CreditRiskType,
            result.CreatedOn,
            result.UpdatedOn);
    }

    [HttpGet("{brandId:guid}/histories", Name = "BrandGameCredit_History")]
    public async IAsyncEnumerable<BrandGameCreditViewModel> QueryAsync(
        [FromServices] IQueryService<BrandGameCreditHistoryQuery, BrandGameCreditResult> service,
        Guid brandId)
    {
        var results = await service.QueryAsync(
            query: new BrandGameCreditHistoryQuery(brandId),
            cancellationToken: HttpContext.RequestAborted)
            .ToArrayAsync(
            cancellationToken: HttpContext.RequestAborted)
            .ConfigureAwait(false)
            ?? Enumerable.Empty<BrandGameCreditResult>();

        if (!results.Any())
        {
            yield break;
        }

        foreach (var result in results)
        {
            yield return new BrandGameCreditViewModel(
                ResellerId: result.ResellerId,
                ResellerCode: result.ResellerCode,
                LicenseeId: result.LicenseeId,
                LicenseeCode: result.LicenseeCode,
                BrandId: result.BrandId,
                BrandCode: result.BrandCode,
                Month: result.Month,
                InitialAmount: result.InitialAmount,
                CreditAmount: result.CreditAmount,
                DebitAmount: result.DebitAmount,
                ProfitAmount: result.ProfitAmount,
                CreditLimit: result.CreditLimit,
                CreditUsed: result.CreditUsed,
                CreditUsedPer: result.CreditUsedPer,
                CreditAvailable: result.CreditAvailable,
                CreditAvailablePer: result.CreditAvailablePer,
                CreditRiskType: result.CreditRiskType,
                CreatedOn: result.CreatedOn,
                UpdatedOn: result.UpdatedOn);
        }
    }
}

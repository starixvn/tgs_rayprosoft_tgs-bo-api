using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;
using Tgs.ServiceUtility;
using DocumentFormat.OpenXml.Bibliography;
using TGS.Apis.BackendOffice.Controllers.Messages.Licensee;
using TGS.Apis.BackendOffice.Controllers.Messages.Reseller;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Models.GameProvider;
using Microsoft.AspNetCore.Authorization;

namespace TGS.Apis.BackendOffice.Controllers.BrandGameCredits;

[Route("api/[controller]")]
[ApiController]
public class BrandGameCreditProfitController : ControllerBase
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [HttpGet("{brandId:guid}/query", Name = "BrandGameCreditProfit_Query")]
    public async IAsyncEnumerable<BrandGameCreditProfitViewModel> QueryAsync(
        [FromServices] IQueryService<BrandGameCreditProfitQuery, BrandGameCreditProfitResult> service,
        Guid brandId,
        [FromQuery] BrandGameCreditProfitQueryViewModel source)
    {
        var results = await service.QueryAsync(
            query: new BrandGameCreditProfitQuery(brandId, source.Month),
            cancellationToken: HttpContext.RequestAborted)
            .ToArrayAsync(
            cancellationToken: HttpContext.RequestAborted)
            .ConfigureAwait(false)
            ?? Enumerable.Empty<BrandGameCreditProfitResult>();

        if (!results.Any())
        {
            yield break;
        }

        foreach (var result in results)
        {
            yield return new BrandGameCreditProfitViewModel(
                ResellerId: result.ResellerId,
                ResellerCode: result.ResellerCode,
                LicenseeId: result.LicenseeId,
                LicenseeCode: result.LicenseeCode,
                BrandId: result.BrandId,
                BrandCode: result.BrandCode,
                Month: result.Month,
                GameProviderId: result.GameProviderId,
                GameProviderCode: result.GameProviderCode,
                GameProviderName: result.GameProviderName,
                Count: result.Count,
                Bet: result.Bet,
                ValidBet: result.ValidBet,
                Settle: result.Settle,
                Commission: result.Commission,
                Turnover: result.Turnover,
                WinLoss: result.WinLoss,
                ProfitAmount: result.ProfitAmount);
        }
    }
}

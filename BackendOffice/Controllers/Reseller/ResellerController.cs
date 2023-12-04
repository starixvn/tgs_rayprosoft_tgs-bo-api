using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TGS.Apis.BackendOffice.Controllers.Messages.Reseller;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Reseller;

[Route("api/[controller]")]
[ApiController]
public partial class ResellerController(IResellerService service) : BaseContextController
{
    [HttpPost]
    [Description("Create a Reseller")]
    public async Task<MessageResponse> CreateResellerAsync(CreateResellerRequest request)
    {
        if (await service.ExistsAsync(x => x.Name == request.Name))
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Reseller.NameExists);
        }

#pragma warning disable CA1862 // 使用 'StringComparison' 方法多載來執行不區分大小寫的字串比較
        if (await service.ExistsAsync(x => x.Code.ToLower() == request.Code.ToLower()))
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Reseller.CodeExists);
        }
#pragma warning restore CA1862 // 使用 'StringComparison' 方法多載來執行不區分大小寫的字串比較

        await service.CreateResellerAsync(request, Username);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpGet("{id}")]
    [Description("Get a Reseller")]
    public async Task<GetResellerResponse> GetResellerAsync(Guid id)
    {
        var reseller = await service.GetResellerAsync(x => x.Id == id,
            x => x.Include(i => i.Currencies)
                .Include(i => i.Languages)
                .Include(i => i.GameProviders)
                .Where(ResellerId != null, r => r.Id == ResellerId));

        return reseller == null
            ? CreateInvalidResponse<GetResellerResponse>(Resources.Reseller.NotFound)
            : new GetResellerResponse
            {
                Valid = true,
                Reseller = new ResellerModel
                {
                    Id = reseller.Id,
                    Code = reseller.Code,
                    Name = reseller.Name,
                    BusinessType = reseller.BusinessType,
                    Status = reseller.Status,
                    Currencies = reseller.Currencies
                    .Select(x => new ResellerCurrencyModel
                    {
                        Code = x.CurrencyCode,
                        Name = x.Currency.Name
                    }).ToArray(),
                    Languages = reseller.Languages
                    .Select(x => new ResellerLanguageModel
                    {
                        CultureCode = x.CultureCode,
                        Name = x.Language.Name
                    }).ToArray(),
                    GameProviders = reseller.GameProviders
                    .Select(x => new ResellerGameProviderModel
                    {
                        GameProviderId = x.GameProviderId,
                        Name = x.GameProvider.Name
                    }).ToArray(),
                    BetLimits = reseller.BetLimits
                    .Select(x => new ResellerBetLimitModel
                    {
                        Id = x.BetLimitId,
                        CurrencyCode = x.BetLimit.CurrencyCode,
                        MinBet = x.BetLimit.MinBet,
                        MaxBet = x.BetLimit.MaxBet,
                        MaxBetPerMatch = x.BetLimit.MaxBetPerMatch
                    }).ToArray(),

                    Description = reseller.Description,
                    OperationalEmail = reseller.OperationalEmail,
                    CreatedOn = reseller.CreatedOn,
                    UpdatedOn = reseller.UpdatedOn
                }
            };
    }

    [HttpGet]
    [Description("Get resellers: 總公司列表")]
    public Task<PagedResponse<ResellerModel>> GetResellersAsync([FromQuery] GetResellersRequest request)
    {
        return service.GetResellers(request)
            .Where(ResellerId, x => x.Id == ResellerId)
            .Select(x => new ResellerModel
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                BusinessType = x.BusinessType,
                Status = x.Status,
                Description = x.Description,
                OperationalEmail = x.OperationalEmail,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn
            }).CreatePagedResponseAsync(request);
    }

    [HttpPut("{id}")]
    [Description("Update a Reseller")]
    public async Task<MessageResponse> UpdateResellerAsync(Guid id, UpdateResellerRequest request)
    {
        var reseller = await service.GetResellerAsync(q => q.Id == id);

        if (reseller == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Reseller.NotFound);
        }

        if (await service.ExistsAsync(x => x.Id != id && x.Name == request.Name))
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Reseller.NameExists);
        }

        if (await service.ExistsAsync(x => x.Id != id && x.Code == request.Code))
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Reseller.CodeExists);
        }

        await service.UpdateResellerAsync(reseller, request, Username);

        return CreateValidResponse<MessageResponse>();
    }
}

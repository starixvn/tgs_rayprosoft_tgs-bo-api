using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TGS.Apis.BackendOffice.Controllers.Messages.Licensee;
using TGS.Apis.BackendOffice.DataService.Services;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Licensee;

[Route("api/[controller]")]
[ApiController]
public partial class LicenseeController : BaseContextController
{
    private readonly ILicenseeService _service;

    public LicenseeController(ILicenseeService service)
    {
        _service = service;
    }

    [HttpPost]
    [Description("Create a Licensee")]
    public async Task<MessageResponse> CreateLicenseeAsync(CreateLicenseeRequest request)
    {
        if (await _service.ExistsAsync(x => x.Name == request.Name))
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Licensee.NameExists);
        }

#pragma warning disable CA1862 // 使用 'StringComparison' 方法多載來執行不區分大小寫的字串比較
        if (await _service.ExistsAsync(x => x.Code.ToLower() == request.Code.ToLower()))
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Licensee.CodeExists);
        }
#pragma warning restore CA1862 // 使用 'StringComparison' 方法多載來執行不區分大小寫的字串比較

        await _service.CreateLicenseeAsync(request, Username);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpGet("{id}")]
    [Description("Get a Licensee")]
    public async Task<GetLicenseeResponse> GetLicenseeAsync(Guid id)
    {
        var licensee = await _service.GetLicenseeAsync(x => x.Id == id,
            x => x.Include(i => i.Currencies)
                .Include(i => i.Languages)
                .Include(i => i.GameProviders)
                .Where(ResellerId != null, x => x.ResellerId == ResellerId)
                .Where(LicenseeId != null, x => x.Id == LicenseeId), true);

        return licensee == null
            ? CreateInvalidResponse<GetLicenseeResponse>(Resources.Licensee.NotFound)
            : new GetLicenseeResponse
            {
                Valid = true,
                Licensee = new LicenseeModel
                {
                    Id = licensee.Id,
                    ResellerId = licensee.ResellerId,
                    BusinessType = licensee.Reseller.BusinessType,
                    Code = licensee.Code,
                    Name = licensee.Name,
                    Status = licensee.Status,
                    Currencies = licensee.Currencies
                    .Select(x => new LicenseeCurrencyModel
                    {
                        Code = x.CurrencyCode,
                        Name = x.Currency.Name
                    }).ToArray(),
                    Languages = licensee.Languages
                    .Select(x => new LicenseeLanguageModel
                    {
                        CultureCode = x.CultureCode,
                        Name = x.Language.Name
                    }).ToArray(),
                    GameProviders = licensee.GameProviders
                    .Select(x => new LicenseeGameProviderModel
                    {
                        GameProviderId = x.GameProviderId,
                        Name = x.GameProvider.Name
                    }).ToArray(),
                    BetLimits = licensee.BetLimits
                    .Select(x => new LicenseeBetLimitModel
                    {
                        Id = x.BetLimitId,
                        CurrencyCode = x.BetLimit.CurrencyCode,
                        MinBet = x.BetLimit.MinBet,
                        MaxBet = x.BetLimit.MaxBet,
                        MaxBetPerMatch = x.BetLimit.MaxBetPerMatch
                    }).ToArray(),

                    Description = licensee.Description,
                    OperationalEmail = licensee.OperationalEmail,
                    CreatedOn = licensee.CreatedOn,
                    UpdatedOn = licensee.UpdatedOn
                }
            };
    }

    [HttpGet]
    [Description("Get licensees: 營運商列表")]
    public Task<PagedResponse<LicenseeModel>> GetLicenseesAsync([FromQuery] GetLicenseesRequest request)
    {
        return _service.GetLicensees(request)
            .Where(request.ResellerId, x => x.ResellerId == request.ResellerId)
            .Where(ResellerId, x => x.ResellerId == ResellerId)
            .Where(LicenseeId, x => x.Id == LicenseeId)
            .Select(x => new LicenseeModel
            {
                Id = x.Id,
                ResellerId = x.ResellerId,
                BusinessType = x.Reseller.BusinessType,
                Name = x.Name,
                Code = x.Code,
                Status = x.Status,
                Description = x.Description,
                OperationalEmail = x.OperationalEmail,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn
            }).CreatePagedResponseAsync(request);
    }

    [HttpPut("{id}")]
    [Description("Update a Licensee")]
    public async Task<MessageResponse> UpdateLicenseeAsync(Guid id, UpdateLicenseeRequest request)
    {
        var licensee = await _service.GetLicenseeAsync(q => q.Id == id, tracking: true);

        if (licensee == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Licensee.NotFound);
        }

        if (await _service.ExistsAsync(x => x.Id != id && x.Name == request.Name))
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Licensee.NameExists);
        }

        if (await _service.ExistsAsync(x => x.Id != id && x.Code == request.Code))
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Licensee.CodeExists);
        }

        await _service.UpdateLicenseeAsync(licensee, request, Username);

        return CreateValidResponse<MessageResponse>();
    }
}

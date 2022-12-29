using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Currency;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.CurrencyHandlers;

public class GetCurrenciesQueryHandler : Handler<GetCurrenciesRequest, PagedResponse<CurrencyModel>>
{
    private readonly ICurrencyService _service;

    public GetCurrenciesQueryHandler(ICurrencyService service, IHttpContextAccessor accessor)
        : base(accessor)
    {
        _service = service;
    }

    public override Task<PagedResponse<CurrencyModel>> Handle(GetCurrenciesRequest request,
        CancellationToken cancellationToken) =>
        _service.GetCurrencieQueries(q => q.Where(ResellerId != null,
                    r => r.ResellerCurrencies.Select(x => x.ResellerId).Contains(ResellerId.Value))
                .Where(LicenseeId != null,
                    l => l.LicenseeCurrencies.Select(x => x.LicenseeId).Contains(LicenseeId.Value))
                .Where(BrandId != null, b => b.BrandCurrencies.Select(x => x.BrandId).Contains(BrandId.Value))
                .Where(request.Code, g => g.Code == request.Code)
                .Where(request.Name, g => g.Name == request.Name))
            .Select(x => new CurrencyModel
            {
                Code = x.Code,
                Name = x.Name,
                NativeName = x.NativeName,
                Symbol = x.Symbol,
                CreatedOn = x.CreatedOn,
                CreatedByUsername = x.CreatedByUsername,
                UpdatedOn = x.UpdatedOn,
                UpdatedByUsername = x.UpdatedByUsername
            }).CreatePagedResponseAsync(request);
}

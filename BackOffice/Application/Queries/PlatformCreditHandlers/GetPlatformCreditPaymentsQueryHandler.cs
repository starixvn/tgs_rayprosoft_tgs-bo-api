using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.PlatformCredit;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlatformCreditHandlers;

public class GetPlatformCreditPaymentsQueryHandler(
    IPlatformCreditService service,
    IHttpContextAccessor accessor)
    : Handler<GetPlatformCreditPaymentRequest, PagedResponse<PlatformCreditPaymentModel>>(accessor)
{
    public override Task<PagedResponse<PlatformCreditPaymentModel>> Handle(
        GetPlatformCreditPaymentRequest request,
        CancellationToken cancellationToken)
        => service.GetPlatformCreditPaymentQueries(q => q
            .Where(x => x.PlatformCreditId == request.PlatformCreditId))
            .Select(x => new PlatformCreditPaymentModel
            {
                Amount = x.PayAmount,
                Balance = x.Balance,
                CreatedOn = x.CreatedOn
            })
            .CreatePagedResponseAsync(request);
}

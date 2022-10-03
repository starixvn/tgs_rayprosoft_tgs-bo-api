using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.PlatformCredit;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlatformCreditHandlers;

public class
    GetPlatformCreditPaymentsQueryHandler : Handler<GetPlatformCreditPaymentRequest,
        PagedResponse<PlatformCreditPaymentModel>>
{
    private readonly IPlatformCreditService _service;

    public GetPlatformCreditPaymentsQueryHandler(IPlatformCreditService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
    }

    public override Task<PagedResponse<PlatformCreditPaymentModel>> Handle(GetPlatformCreditPaymentRequest request,
        CancellationToken cancellationToken) =>
        _service.GetPlatformCreditPaymentQueries(q => q
                .Where(x => x.PlatformCreditId == request.PlatformCreditId))
            .Select(x => new PlatformCreditPaymentModel
            {
                Amount = x.PayAmount,
                Balance = x.Balance,
                CreatedOn = x.CreatedOn
            }).CreatePagedResponseAsync(request);
}
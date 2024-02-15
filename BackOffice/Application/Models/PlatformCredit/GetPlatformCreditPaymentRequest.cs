using System;
using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.PlatformCredit;

public class GetPlatformCreditPaymentRequest : PageableRequest, IRequest<PagedResponse<PlatformCreditPaymentModel>>
{
    public Guid PlatformCreditId { get; set; }
}

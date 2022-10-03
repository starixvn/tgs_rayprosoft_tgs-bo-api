using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.PlatformCredit;

public class GetPlatformCreditsRequest : PageableRequest, IRequest<PagedResponse<PlatformCreditModel>>
{
    public string Keyword { get; set; }
    public string LicenseeName { get; set; }
    public Guid? BrandId { get; set; }
}
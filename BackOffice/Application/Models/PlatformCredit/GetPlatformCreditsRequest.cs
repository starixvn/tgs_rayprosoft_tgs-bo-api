using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.PlatformCredit;

public class GetPlatformCreditsRequest : PageableRequest, IRequest<PagedResponse<PlatformCreditModel>>
{
    public Guid? BrandId { get; set; }

    public string Keyword { get; set; }

    public string LicenseeName { get; set; }
}

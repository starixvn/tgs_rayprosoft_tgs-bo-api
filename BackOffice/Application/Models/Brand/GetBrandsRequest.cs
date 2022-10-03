using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public class GetBrandsRequest : PageableRequest, IRequest<PagedResponse<BrandModel>>
{
    public string Keyword { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? LicenseeId { get; set; }
    public Guid? ResellerId { get; set; }
}
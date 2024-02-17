using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class GetBrandGameProvidersRequest : PageableRequest, IRequest<PagedResponse<BrandGameProviderModel>>
{
    public Guid? BrandId { get; set; }

    public GameType? GameType { get; set; }

    public string Keyword { get; set; }
}

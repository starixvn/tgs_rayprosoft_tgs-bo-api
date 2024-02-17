using System;
using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.SubGameProvider;

public class GetSubGameProvidersRequest : PageableRequest, IRequest<PagedResponse<SubGameProviderModel>>
{
    public Guid GameProviderId { get; set; }
}

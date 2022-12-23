using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.SubGameProvider;

public class GetSubGameProvidersRequest : PageableRequest, IRequest<PagedResponse<SubGameProviderModel>>
{
    public Guid GameProviderId { get; set; }
}

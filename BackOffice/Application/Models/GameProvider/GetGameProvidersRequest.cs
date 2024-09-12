using MediatR;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.GameProvider;

public class GetGameProvidersRequest
    : PageableRequest
    , IRequest<PagedResponse<GameProviderBaseModel>>
{
    public string Code { get; set; }

    public string Name { get; set; }

    public Status? Status { get; set; }
}

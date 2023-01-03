using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.GameProvider;

public class GetGameProvidersRequest : PageableRequest, IRequest<PagedResponse<GameProviderBaseModel>>
{
    public string Code { get; set; }

    public string Name { get; set; }
}

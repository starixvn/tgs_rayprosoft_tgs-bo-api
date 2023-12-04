using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProvider;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.GameProviderHandlers;

public class CreateGameProviderHandler(IGameProviderService service, IHttpContextAccessor accessor) : Handler<CreateGameProviderRequest, MessageResponse>(accessor)
{
    private readonly IGameProviderService _service = service;

    public override async Task<MessageResponse> Handle(CreateGameProviderRequest request,
        CancellationToken cancellationToken)
    {
        if (await _service.ExistsAsync(x => request.Id != x.Id && x.Code == request.Code))
        {
            return CreateInvalidResponse<MessageResponse>(GameProvider.CodeExisted);
        }

        if (await _service.ExistsAsync(x => request.Id != x.Id && x.Name == request.Name))
        {
            return CreateInvalidResponse<MessageResponse>(GameProvider.NameExisted);
        }

        await _service.CreateGameProviderAsync(request);

        return CreateValidResponse<MessageResponse>();
    }
}

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProvider;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.GameProviderHandlers;

public class UpdateGameProviderHandler : Handler<UpdateGameProviderRequest, MessageResponse>
{
    private readonly IGameProviderService _service;

    public UpdateGameProviderHandler(IGameProviderService service,
        IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(UpdateGameProviderRequest request,
        CancellationToken cancellationToken)
    {
        var gameProvider = await _service.GetGameProviderAsync(x => x.Id == request.Id);

        if (gameProvider == null)
        {
            return CreateInvalidResponse<MessageResponse>(GameProvider.NotFound);
        }

        if (await _service.ExistsAsync(x => request.Id != x.Id && x.Name == request.Name))
        {
            return CreateInvalidResponse<MessageResponse>(GameProvider.NameExisted);
        }

        if (await _service.ExistsAsync(x => request.Id != x.Id && x.Code == request.Code))
        {
            return CreateInvalidResponse<MessageResponse>(GameProvider.CodeExisted);
        }

        await _service.UpdateGameProviderAsync(gameProvider, request);

        return CreateValidResponse<MessageResponse>();
    }
}
using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.GameProvider;

public class GetGameProviderRequest : IRequest<GetGameProviderResponse>
{
    public Guid Id { get; set; }

    public bool IncludeAllGames { get; set; } = false;

    public GetGameProviderRequest(Guid id)
    {
        Id = id;
    }
}

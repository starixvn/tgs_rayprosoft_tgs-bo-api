using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.GameProvider;

public class GetGameProviderRequest(Guid id) : IRequest<GetGameProviderResponse>
{
    public Guid Id { get; set; } = id;

    public bool IncludeAllGames { get; set; } = false;
}

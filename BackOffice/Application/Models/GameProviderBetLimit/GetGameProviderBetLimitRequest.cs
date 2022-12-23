using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class GetGameProviderBetLimitRequest : IRequest<GetGameProviderBetLimitResponse>
{
    public Guid Id { get; set; }

    public GetGameProviderBetLimitRequest(Guid id)
    {
        Id = id;
    }
}

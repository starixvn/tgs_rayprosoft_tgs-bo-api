using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class GetGameProviderBetLimitRequest : IRequest<GetGameProviderBetLimitResponse>
{
    public GetGameProviderBetLimitRequest(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}
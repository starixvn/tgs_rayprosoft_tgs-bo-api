using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class GetGameProviderBetLimitRequest(Guid id) : IRequest<GetGameProviderBetLimitResponse>
{
    public Guid Id { get; set; } = id;
}

using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class GetInUseBrandsRequest(Guid id) : IRequest<GetInUseBrandsResponse>
{
    public Guid Id { get; set; } = id;
}

using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class GetInUseBrandsRequest : IRequest<GetInUseBrandsResponse>
{
    public Guid Id { get; set; }

    public GetInUseBrandsRequest(Guid id)
    {
        Id = id;
    }
}

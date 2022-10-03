using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class GetInUseBrandsRequest : IRequest<GetInUseBrandsResponse>
{
    public GetInUseBrandsRequest(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}
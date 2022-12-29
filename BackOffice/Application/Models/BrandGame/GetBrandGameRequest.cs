using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.BrandGame;

public class GetBrandGameRequest : IRequest<GetBrandGameResponse>
{
    public Guid Id { get; set; }

    public GetBrandGameRequest(Guid id)
    {
        Id = id;
    }
}

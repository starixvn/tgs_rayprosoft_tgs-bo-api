using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.BrandGame;

public class GetBrandGameRequest : IRequest<GetBrandGameResponse>
{
    public GetBrandGameRequest(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}
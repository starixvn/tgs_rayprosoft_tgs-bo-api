using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.BrandGame;

public class GetBrandGameRequest(Guid id) : IRequest<GetBrandGameResponse>
{
    public Guid Id { get; set; } = id;
}

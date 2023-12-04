using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public class GetBrandRequest(Guid id) : IRequest<GetBrandResponse>
{
    public Guid Id { get; set; } = id;
}

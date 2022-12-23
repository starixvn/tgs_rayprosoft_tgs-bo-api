using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public class GetBrandRequest : IRequest<GetBrandResponse>
{
    public Guid Id { get; set; }

    public GetBrandRequest(Guid id)
    {
        Id = id;
    }
}

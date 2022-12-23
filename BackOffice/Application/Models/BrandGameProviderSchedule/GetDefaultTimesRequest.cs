using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class GetDefaultTimesRequest : IRequest<GetDefaultTimesResponse>
{
    public Guid BrandGameProviderId { get; set; }

    public GetDefaultTimesRequest(Guid id)
    {
        BrandGameProviderId = id;
    }
}

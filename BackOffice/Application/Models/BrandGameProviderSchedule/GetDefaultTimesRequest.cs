using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class GetDefaultTimesRequest : IRequest<GetDefaultTimesResponse>
{
    public GetDefaultTimesRequest(Guid id)
    {
        BrandGameProviderId = id;
    }

    public Guid BrandGameProviderId { get; set; }
}
using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class GetDefaultTimesRequest(Guid id) : IRequest<GetDefaultTimesResponse>
{
    public Guid BrandGameProviderId { get; set; } = id;
}

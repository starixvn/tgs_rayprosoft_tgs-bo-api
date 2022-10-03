using MediatR;

namespace TGS.Services.BackOffice.Application.Models.BetLimit;

public class GetBetLimitRequest : IRequest<GetBetLimitResponse>
{
    public GetBetLimitRequest(int id)
    {
        Id = id;
    }

    public int Id { get; set; }
}
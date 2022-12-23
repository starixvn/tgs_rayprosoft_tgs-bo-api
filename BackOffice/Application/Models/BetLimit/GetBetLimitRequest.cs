using MediatR;

namespace TGS.Services.BackOffice.Application.Models.BetLimit;

public class GetBetLimitRequest : IRequest<GetBetLimitResponse>
{
    public int Id { get; set; }

    public GetBetLimitRequest(int id)
    {
        Id = id;
    }
}

using MediatR;

namespace TGS.Services.BackOffice.Application.Models.BetLimit;

public class GetBetLimitRequest(int id) : IRequest<GetBetLimitResponse>
{
    public int Id { get; set; } = id;
}

using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class GetPlayerRequest(string playerCode) : IRequest<GetPlayerResponse>
{
    public string PlayerCode { get; set; } = playerCode;
}

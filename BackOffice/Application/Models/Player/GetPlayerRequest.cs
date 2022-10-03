using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class GetPlayerRequest : IRequest<GetPlayerResponse>
{
    public GetPlayerRequest(string playerCode)
    {
        PlayerCode = playerCode;
    }

    public string PlayerCode { get; set; }
}
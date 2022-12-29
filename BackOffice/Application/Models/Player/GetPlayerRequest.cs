using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class GetPlayerRequest : IRequest<GetPlayerResponse>
{
    public string PlayerCode { get; set; }

    public GetPlayerRequest(string playerCode)
    {
        PlayerCode = playerCode;
    }
}

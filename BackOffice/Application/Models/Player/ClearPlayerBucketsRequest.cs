using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class ClearPlayerBucketsRequest : BaseRequest, IRequest<ClearPlayerBucketsResponse>
{
    public string PlayerCode { get; set; }

    public ClearPlayerBucketsRequest(string playerCode)
    {
        PlayerCode = playerCode;
    }
}

using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class ClearPlayerBucketsRequest(string playerCode) : BaseRequest, IRequest<ClearPlayerBucketsResponse>
{
    public string PlayerCode { get; set; } = playerCode;
}

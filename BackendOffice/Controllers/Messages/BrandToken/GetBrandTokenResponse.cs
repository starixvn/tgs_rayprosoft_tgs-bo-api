using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandToken;

public class GetBrandTokenResponse : MessageResponse
{
    public string Token { get; init; }
}

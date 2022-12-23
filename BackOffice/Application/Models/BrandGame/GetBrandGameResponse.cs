using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandGame;

public class GetBrandGameResponse : MessageResponse
{
    public BrandGameResponseModel BrandGame { get; set; }
}

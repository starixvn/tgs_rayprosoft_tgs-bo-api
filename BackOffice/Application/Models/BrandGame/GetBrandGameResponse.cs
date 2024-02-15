using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BrandGame;

public class GetBrandGameResponse : MessageResponse
{
    public BrandGameResponseModel BrandGame { get; set; }
}

using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public class GetBrandResponse : MessageResponse
{
    public BrandModel Brand { get; set; }
}
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class GetInUseBrandsResponse : MessageResponse
{
    public string[] BrandNames { get; set; }
}
namespace TGS.Services.BackOffice.Application.Models.BrandAllAgent;

public record GetBrandAllAgentResponse
{
    public bool Switch { get; set; }

    public BrandAllAgentGameTypeModel[] BrandAllAgentGameTypes { get; set; }
}
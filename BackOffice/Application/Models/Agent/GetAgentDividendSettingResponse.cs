using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class GetAgentDividendSettingResponse : MessageResponse
{
    public decimal? DividendPercentage { get; set; }
}

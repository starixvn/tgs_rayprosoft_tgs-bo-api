using Tgs.Agent.Client.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class SetOneLayerAgentInfoViewModel
{
    public string ParentUsername { get; init; }

    public decimal DividendPercentage { get; init; }

    public AgentType Type { get; init; }
}

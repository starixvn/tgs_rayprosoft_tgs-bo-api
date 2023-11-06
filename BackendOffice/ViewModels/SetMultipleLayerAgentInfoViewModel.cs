using Tgs.Agent.Client.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Agent;

namespace TGS.Apis.BackendOffice.ViewModels;

public class SetMultipleLayerAgentInfoViewModel
{
    public string ParentUsername { get; init; }
    public AgentDividendConditionModel[] AgentDividendConditions { get; init; }

    public AgentType Type { get; init; }
}

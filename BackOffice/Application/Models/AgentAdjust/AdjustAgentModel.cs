using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.AgentAdjust;

public class AdjustAgentModel
{
    public decimal Balance { get; set; }

    public AgentLevelType LevelType { get; set; }

    public AgentType Type { get; set; }

    public string Username { get; set; }
}

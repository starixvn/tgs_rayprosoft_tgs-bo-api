namespace TGS.Services.BackOffice.Application.Models.Agent;

public class AgentDividendConditionModel
{
    public int ActiveNumber { get; set; }

    public decimal DividendPercentage { get; set; }

    public decimal NetProfit { get; set; }

    public int Rank { get; set; }
}

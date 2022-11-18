using System.Text.Json.Serialization;

namespace TGS.Services.BackOffice.Domain;

public class DepositAccumulateRouletteSettings
{
    [JsonPropertyName("compaignDetail")]
    public DepositAccumulateRouletteCollection CampaignDetail { get; init; } = default!;
}

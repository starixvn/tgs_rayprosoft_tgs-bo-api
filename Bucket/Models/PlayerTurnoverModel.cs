namespace TGS.Services.PlayerBucket.Models;

public record PlayerTurnoverModel
{
    public string PlayerCode { get; init; }
    public decimal Turnover { get; init; }
}

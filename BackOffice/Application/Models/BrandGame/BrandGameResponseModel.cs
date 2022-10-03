using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.BrandGame;

public class BrandGameResponseModel
{
    public Guid Id { get; set; }
    public int GameId { get; set; }
    public string GameProviderName { get; set; }
    public string GameName { get; set; }
    public GameStatus Status { get; set; }
    public GameType GameType { get; set; }
    public GamePlatformType Platform { get; set; }
    public bool SupportHtml5 { get; set; }
    public decimal EffectiveBettingPercentage { get; set; }
    public bool Rebate { get; set; }
}
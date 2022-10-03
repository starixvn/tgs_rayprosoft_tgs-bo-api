using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.BrandGame;

public class BrandGameModel
{
    public Guid Id { get; set; }
    public int GameId { get; set; }
    public string GameProviderName { get; set; }
    public int DisplayOrder { get; set; }
    public string GameName { get; set; }
    public GameType GameType { get; set; }
    public GamePlatformType Platform { get; set; }
    public GameStatus Status { get; set; }
    public bool IsHot { get; set; }
    public bool IsNew { get; set; }
    public bool IsRecommend { get; set; }
}
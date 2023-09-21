using System;

namespace TGS.Services.BackOffice.Domain;

public record TopTenGameInfo
{
    public Guid GameProviderId { get; set; }
    public int GameId { get; set; }
    public decimal Bet { get; set; }
    public decimal WinLoss { get; set; }
}

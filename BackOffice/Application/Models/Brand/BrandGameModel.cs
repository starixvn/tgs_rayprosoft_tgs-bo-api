using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public class BrandGameModel
{
    public int GameId { get; set; }

    public string GameName { get; set; }

    public Guid GameProviderId { get; set; }

    public GameType GameType { get; set; }
}

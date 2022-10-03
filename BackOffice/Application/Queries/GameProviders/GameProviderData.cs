using System;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

public class GameProviderData
{
    public Guid GameProviderId { get; set; }
    public string GameProviderName { get; set; }
    public string Localization { get; set; }
}
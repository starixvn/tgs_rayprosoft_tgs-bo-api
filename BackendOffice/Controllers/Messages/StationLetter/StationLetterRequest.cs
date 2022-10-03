using System;
using System.Text.Json;

namespace TGS.Apis.BackendOffice.Controllers.Messages.StationLetter;

public class StationLetterRequest
{
    public bool AssignedAllPlayers { get; set; }

    public Guid BrandId { get; set; }

    public string Content { get; set; }

    public DateTimeOffset From { get; set; }

    public JsonDocument Localization { get; set; }

    public string[] PlayerUsernames { get; set; }

    public string Subject { get; set; }

    public DateTimeOffset To { get; set; }
}

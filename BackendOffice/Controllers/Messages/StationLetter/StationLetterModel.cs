using System;
using System.Text.Json;

namespace TGS.Apis.BackendOffice.Controllers.Messages.StationLetter;

public class StationLetterModel
{
    public bool AssignedAllPlayers { get; set; }

    public Guid BrandId { get; set; }

    public string BrandName { get; set; }

    public string Content { get; set; }

    public string CreatedByUsername { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public Guid Id { get; set; }

    public JsonDocument Localization { get; set; }

    public string Remarks { get; set; }

    public string Status { get; set; }

    public string Subject { get; set; }

    public string VerifiedByUsername { get; set; }
}

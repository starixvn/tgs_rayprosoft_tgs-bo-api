using System;
using System.Text.Json;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Announcement;

public class AnnouncementModel
{
    public Guid BrandId { get; set; }

    public string BrandName { get; set; }

    public string Content { get; set; }

    public string CreatedByUsername { get; set; }

    public int DisplayOrder { get; set; }

    public DateTimeOffset From { get; set; }

    public Guid Id { get; set; }

    public JsonDocument Localization { get; set; }

    public string Remarks { get; set; }

    public string Status { get; set; }

    public string Subject { get; set; }

    public DateTimeOffset To { get; set; }

    public string Type { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }

    public string VerifiedByUsername { get; set; }
}

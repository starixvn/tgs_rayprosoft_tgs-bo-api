using System;
using System.Text.Json;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Announcement;

public class AnnouncementRequest
{
    public Guid BrandId { get; set; }

    public string Content { get; set; }

    public DateTimeOffset From { get; set; }

    public JsonDocument Localization { get; set; }

    public string Subject { get; set; }

    public DateTimeOffset To { get; set; }

    public string Type { get; set; }
}

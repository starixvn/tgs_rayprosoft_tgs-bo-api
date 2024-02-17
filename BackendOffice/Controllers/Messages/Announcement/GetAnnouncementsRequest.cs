using System;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Announcement;

public class GetAnnouncementsRequest : PageableRequest
{
    public Guid? BrandId { get; set; }

    public AnnounceStatus[] Status { get; set; }

    public string Type { get; set; }
}

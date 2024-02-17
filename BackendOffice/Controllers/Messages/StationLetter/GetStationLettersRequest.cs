using System;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.StationLetter;

public class GetStationLettersRequest : PageableRequest
{
    public Guid? BrandId { get; set; }

    public StationLetterStatus Status { get; set; }
}

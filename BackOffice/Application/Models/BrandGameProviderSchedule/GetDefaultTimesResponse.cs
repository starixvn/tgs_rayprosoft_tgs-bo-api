using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class GetDefaultTimesResponse : MessageResponse
{
    public DefaultTimeModel[] DefaultTimes { get; set; }
}
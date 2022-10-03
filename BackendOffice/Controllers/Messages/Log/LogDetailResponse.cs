using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Log;

public class LogDetailResponse : MessageResponse
{
    public LogDetailModel LogDetail { get; set; }
}
using System.Net.Http;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages;

public sealed class ValidateExcelResponse : MessageResponse
{
    public ValidateExcelResponse()
    {
        Valid = true;
    }

    public ValidateExcelResponse(string message, HttpContent httpContent = null)
    {
        Valid = false;
        Message = message;
        HttpContent = httpContent;
    }

    public HttpContent HttpContent { get; set; }
}

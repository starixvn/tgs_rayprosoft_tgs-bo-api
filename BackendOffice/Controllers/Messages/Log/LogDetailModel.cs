namespace TGS.Apis.BackendOffice.Controllers.Messages.Log;

public class LogDetailModel
{
    public string RequestHeaders { get; set; }
    public string RequestContent { get; set; }
    public string ResponseHeaders { get; set; }
    public string ResponseContent { get; set; }
}
namespace TGS.Services.BackOffice.Domain;

public class MessageResponse
{
    public string[] Errors { get; set; }

    public string Message { get; set; }

    public bool Valid { get; set; }

    public MessageResponse()
    {
        Valid = false;
        Message = string.Empty;
        Errors = [];
    }
}

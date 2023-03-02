namespace TGS.Apis.BackendOffice.ViewModels;

public class ResponseViewModel<TData>
{
    public TData Data { get; set; }

    public string[] Errors { get; set; }

    public string Message { get; set; }

    public bool Valid { get; set; }
}

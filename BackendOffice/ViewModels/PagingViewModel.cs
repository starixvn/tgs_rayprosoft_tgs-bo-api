namespace TGS.Apis.BackendOffice.ViewModels;

public class PagingViewModel
{
    public bool IsDesc { get; set; }

    public int Page { get; set; }

    public int Rows { get; set; }

    public string SortBy { get; set; }
}

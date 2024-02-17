namespace TGS.Services.BackOffice.Domain;

public class PageableRequest
{
    public bool IsDesc => OrderBy == Core.Models.Messages.SortOrder.Descending;

    public Core.Models.Messages.SortOrder OrderBy { get; set; }

    public int Page { get; set; }

    public int Rows { get; set; }

    public bool ShowAll { get; set; }

    public string SortBy { get; set; }

    public Core.Models.Messages.SortOption GetSortOption() => new(SortBy, IsDesc);
}

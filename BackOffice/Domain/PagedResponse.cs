namespace TGS.Services.BackOffice.Domain;

public class PagedResponse<T> : MessageResponse
{
    public T[] Items { get; set; }

    public int TotalCount { get; set; }

    public PagedResponse()
    {
    }

    public PagedResponse(int count, T[] items)
    {
        TotalCount = count;
        Items = items;
    }
}

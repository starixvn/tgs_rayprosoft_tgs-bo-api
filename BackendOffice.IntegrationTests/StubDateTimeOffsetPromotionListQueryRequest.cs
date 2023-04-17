#nullable enable

using System.Data.SqlClient;

namespace BackendOffice.IntegrationTests;

internal class StubDateTimeOffsetPromotionListQueryRequest
{
    public Guid BrandId { get; init; }

    public DateTimeOffset? From { get; init; }

    public SortOrder OrderBy { get; set; }

    public int Page { get; set; }

    public int Rows { get; set; }

    public bool ShowAll { get; set; }

    public string? SortBy { get; set; }

    public DateTimeOffset? To { get; init; }
}

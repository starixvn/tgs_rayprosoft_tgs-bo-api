#nullable enable

using System.Data.SqlClient;
using TGS.Core.Models.Enums;

namespace BackendOffice.IntegrationTests;

internal class StubPromotionListQueryRequest
{
    public Guid BrandId { get; init; }

    public DateOnly? From { get; init; }

    public string? Name { get; init; }

    public SortOrder OrderBy { get; set; }

    public int Page { get; set; }

    public string? PromotionType { get; init; }

    public int Rows { get; set; }

    public bool ShowAll { get; set; }

    public string? SortBy { get; set; }

    public SwitchStatus? Status { get; init; }

    public DateOnly? To { get; init; }
}

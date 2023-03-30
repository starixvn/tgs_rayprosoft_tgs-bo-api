#nullable enable

using System;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class BrandListItemViewModel
{
    public Guid Id { get; init; }

    public string Code { get; init; } = default!;

    public string Name { get; init; } = default!;

    public Guid LicenseeId { get; init; }

    public string LicenseeName { get; init; } = default!;

    public Guid ResellerId { get; init; }

    public string ResellerName { get; init; } = default!;

    public BrandStatus Status { get; init; }

    public string? Description { get; init; }

    public string? OperationalEmail { get; init; }

    public DateTime CreatedOn { get; init; }

    public DateTime UpdatedOn { get; init; }
}
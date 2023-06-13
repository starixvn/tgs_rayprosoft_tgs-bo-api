#nullable enable

using System;
using Tgs.CashQueryService;

namespace TGS.Apis.BackendOffice.ViewModels;

public class AdjustRecordListItemViewModel
{
    public required AdjustType AdjustType { get; set; }

    public required decimal Amount { get; set; }

    public required string BrandCode { get; set; }

    public required Guid BrandId { get; set; }

    public required decimal BucketAmount { get; set; }

    public required DateTime CreatedOn { get; set; }

    public required Guid Id { get; set; }

    public required string Operator { get; set; }

    public string? Remark { get; set; }

    public required string UserName { get; set; }

    public required UserType UserType { get; set; }
}

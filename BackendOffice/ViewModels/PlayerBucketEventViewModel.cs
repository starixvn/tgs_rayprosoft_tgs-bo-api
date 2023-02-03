#nullable enable

using System;
using Tgs.Service.PlayerBuckets;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PlayerBucketEventViewModel
{
    public Guid BucketId { get; set; }

    public decimal ChangeAmount { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string? ExternalId { get; set; }

    public Guid Id { get; set; }

    public string? Remark { get; set; }

    public FillType Type { get; set; }
}

#nullable enable

using System;
using Tgs.Service.PlayerBuckets;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PlayerBucketViewModel
{
    public BucketType BucketType { get; set; }

    public DateTime CreatedOn { get; set; }

    public Guid Id { get; set; }

    public string PlayerCode { get; set; } = default!;

    public decimal UncompleteWagering { get; set; }

    public decimal WageringRequirement { get; set; }
}

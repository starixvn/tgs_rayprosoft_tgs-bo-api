#nullable enable

using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class CleanBucketFormViewModel
{
    public decimal? CleaningBalance { get; set; }

    public decimal? CleaningBucketAmount { get; set; }

    public DateTime CreatedOn { get; set; }

    public Guid DepositFormId { get; set; }

    public Guid Id { get; set; }

    public string PlayerCode { get; set; } = default!;

    public decimal ReviewBalance { get; set; }

    public decimal ReviewBucketAmount { get; set; }

    public string Status { get; set; } = default!;
}

using System;
using System.ComponentModel.DataAnnotations;

namespace TGS.Apis.BackendOffice.ViewModels;

public record BrandGameCreditProfitQueryViewModel
{
    [Required]
    [DataType(DataType.Date)]
    public DateOnly Month { get; init; } = default!;
}
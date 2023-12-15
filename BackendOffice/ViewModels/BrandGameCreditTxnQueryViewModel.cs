using System;
using System.ComponentModel.DataAnnotations;
using Tgs.BrandGameCredits.Common;

namespace TGS.Apis.BackendOffice.ViewModels;

public record BrandGameCreditTxnQueryViewModely
{
    [Required]
    [DataType(DataType.DateTime)]
    public DateTimeOffset StartOn { get; init; }
    [Required]
    [DataType(DataType.DateTime)]
    public DateTimeOffset EndOn { get; init; }
    public BrandGameCreditTxnTypes? Type { get; init; }
    [Required]
   
    public int PageNumber { get; init; } = 1;
    [Required]
    public int PageSize { get; init; } = 20;
}

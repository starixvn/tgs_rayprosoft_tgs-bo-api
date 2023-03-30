#nullable enable

using System;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class BrandListQueryViewModel
{
    [FromQuery]
    public Guid? ResellerId { get; set; }

    [FromQuery]
    public Guid? LicenseeId { get; set; }

    [FromQuery]
    public Guid? BrandId { get; set; }

    [FromQuery]
    public string? Keyword { get; set; }

    [FromQuery]
    public BrandStatus? Status { get; set; }
}
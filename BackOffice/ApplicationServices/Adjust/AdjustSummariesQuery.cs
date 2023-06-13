#nullable enable

using System;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;
public record AdjustSummariesQuery(
    (DateTime From, DateTime To) CreatedOnRange);

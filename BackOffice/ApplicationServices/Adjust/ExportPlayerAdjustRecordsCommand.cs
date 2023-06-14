using System;
using System.IO;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;
public record ExportPlayerAdjustRecordsCommand(
    Guid BrandId,
    (DateTime From, DateTime To) CreatedOnRange,
    Stream OutputStream);

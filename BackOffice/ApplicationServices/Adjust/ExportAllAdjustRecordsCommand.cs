using System;
using System.Collections.Generic;
using System.IO;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;
public record ExportAllAdjustRecordsCommand(
    IEnumerable<Guid> BrandIds,
    (DateTime From, DateTime To) CreatedOnRange,
    Stream OutputStream);

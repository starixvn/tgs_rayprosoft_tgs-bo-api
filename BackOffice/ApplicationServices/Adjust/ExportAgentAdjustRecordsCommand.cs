using System;
using System.IO;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;
public record ExportAgentAdjustRecordsCommand(Guid BrandId,
    (DateTime From, DateTime To) CreatedOnRange,
    Stream OutputStream);

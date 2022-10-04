using System;
using System.IO;

#nullable enable

namespace TGS.Services.BackOffice.ApplicationServices.BatchAdjust;
public record ReadBatchAdjustItemsQuery(Guid BrandId, Stream ImportFile);

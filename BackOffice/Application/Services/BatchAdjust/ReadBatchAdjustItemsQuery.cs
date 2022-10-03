using System;
using System.IO;

#nullable enable

namespace TGS.Services.BackOffice.Application.Services.BatchAdjust;
public record ReadBatchAdjustItemsQuery(Guid BrandId, Stream ImportFile);

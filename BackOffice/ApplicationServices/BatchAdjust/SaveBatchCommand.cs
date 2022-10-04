using System.Collections.Generic;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.BatchAdjust;
public record SaveBatchCommand(IAsyncEnumerable<BatchAdjustBalanceItem> Batch);

using System;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

public record GetPlayerCodeByBatchCommand(
    Guid BatchId);
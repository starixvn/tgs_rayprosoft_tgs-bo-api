#nullable enable

using System;

namespace TGS.Services.BackOffice.Domain;
public record BatchAdjustBalanceNotifyBody(Guid BatchId, string? ClientToken);

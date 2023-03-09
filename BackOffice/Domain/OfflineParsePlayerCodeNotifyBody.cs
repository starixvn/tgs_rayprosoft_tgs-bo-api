#nullable enable

using System;

namespace TGS.Services.BackOffice.Domain;
public record OfflineParsePlayerCodeNotifyBody(Guid BatchId, string? ClientToken);

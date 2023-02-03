#nullable enable

namespace TGS.Services.BackOffice.ApplicationServices;
public record AdjustPlayerBucketCommand(string PlayerCode, decimal AdjustAmount, string? Remark);

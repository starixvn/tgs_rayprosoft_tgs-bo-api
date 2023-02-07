#nullable enable

using TGS;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;
public record AdjustPlayerBucketCommand(string PlayerCode, decimal AdjustAmount, string? Remark);

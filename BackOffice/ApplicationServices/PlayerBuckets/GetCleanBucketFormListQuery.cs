using Tgs.Service.PlayerBuckets;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;
public record GetCleanBucketFormListQuery(string PlayerCode, CleanFlowStatus? FlowStatus);

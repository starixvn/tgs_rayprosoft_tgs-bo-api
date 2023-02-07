using System;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;
public record ClearPlayerBucketsCommand(string PlayerCode, Guid AnchorId);

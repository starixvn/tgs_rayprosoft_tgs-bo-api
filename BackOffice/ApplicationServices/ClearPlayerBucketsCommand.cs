using System;

namespace TGS.Services.BackOffice.ApplicationServices;
public record ClearPlayerBucketsCommand(string PlayerCode, Guid AnchorId);

using System;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.OfflineLoginReward;

public class UploadOfflineLoginRewardUserListResponse : MessageResponse
{
    public Guid? BatchId { get; init; }
}
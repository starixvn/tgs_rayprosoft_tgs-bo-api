using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.Resources;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers;

public partial class AnnouncementController
{
    [HttpPut("{id}/status/{status}")]
    [Description("Change an announcement status")]
    public async Task<MessageResponse> ChangeStatusAsync(Guid id, AnnounceStatus status)
    {
        var announcement = await _queries.GetSingleAsync(q => q.Id == id);

        if (announcement == null)
        {
            return CreateInvalidResponse<MessageResponse>(Announcement.NotFound);
        }

        announcement.Status = status;
        announcement.UpdatedOn = DateTimeOffset.Now;
        announcement.UpdatedByUsername = Username;

        await _commands.UpdateAsync(announcement);

        return CreateValidResponse<MessageResponse>();
    }
}
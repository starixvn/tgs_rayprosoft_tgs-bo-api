using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.ViewModels;

public class GetVipLevelsRequest : PageableRequest
{
    public Guid? BrandId { get; set; }
}

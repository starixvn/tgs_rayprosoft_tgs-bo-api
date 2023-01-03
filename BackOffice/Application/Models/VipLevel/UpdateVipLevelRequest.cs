using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.VipLevel;

public class UpdateVipLevelRequest : VipLevelRequest, IRequest<MessageResponse>
{
}

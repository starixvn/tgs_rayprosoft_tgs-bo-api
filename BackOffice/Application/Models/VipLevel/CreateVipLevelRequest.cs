using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.VipLevel;

public class CreateVipLevelRequest : VipLevelRequest, IRequest<MessageResponse>
{
}

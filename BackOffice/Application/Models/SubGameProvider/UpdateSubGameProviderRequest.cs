using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.SubGameProvider;

public record UpdateSubGameProviderRequest : SubGameProviderRequest, IRequest<MessageResponse>
{
}

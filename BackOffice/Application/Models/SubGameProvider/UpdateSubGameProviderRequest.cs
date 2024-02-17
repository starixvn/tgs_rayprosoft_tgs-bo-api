using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.SubGameProvider;

public record UpdateSubGameProviderRequest : SubGameProviderRequest, IRequest<MessageResponse>
{
}

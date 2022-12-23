using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class UpdateBrandGameProviderRequest : BrandGameProviderRequest, IRequest<MessageResponse>
{
}

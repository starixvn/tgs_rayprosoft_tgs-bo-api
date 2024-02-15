using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class UpdateBrandGameProviderRequest : BrandGameProviderRequest, IRequest<MessageResponse>
{
}

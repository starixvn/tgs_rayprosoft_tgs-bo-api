using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models;
public class ChangeOrderBrandGameProviderRequest : IRequest<MessageResponse>
{
    public ChangeOrderModel[] Orders { get; set; }
}

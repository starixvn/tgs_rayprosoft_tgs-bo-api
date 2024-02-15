using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Currency;

public class CreateCurrencyRequest : CurrencyRequest, IRequest<MessageResponse>
{
}

using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Currency;

public class CreateCurrencyRequest : CurrencyRequest, IRequest<MessageResponse>
{
}
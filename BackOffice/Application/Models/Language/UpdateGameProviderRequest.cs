using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Language;

public class UpdateLanguageRequest : LanguageRequest, IRequest<MessageResponse>
{
}

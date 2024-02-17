using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Language;

public class UpdateLanguageRequest : LanguageRequest, IRequest<MessageResponse>
{
}

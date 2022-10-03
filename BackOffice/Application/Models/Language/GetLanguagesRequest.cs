using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Language;

public class GetLanguagesRequest : PageableRequest, IRequest<PagedResponse<LanguageModel>>
{
    public string CultureCode { get; set; }
    public string LanguageCode { get; set; }
}
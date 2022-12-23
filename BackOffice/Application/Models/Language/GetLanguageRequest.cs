using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Language;

public class GetLanguageRequest : IRequest<GetLanguageResponse>
{
    public string CultureCode { get; set; }

    public GetLanguageRequest(string cultureCode)
    {
        CultureCode = cultureCode;
    }
}

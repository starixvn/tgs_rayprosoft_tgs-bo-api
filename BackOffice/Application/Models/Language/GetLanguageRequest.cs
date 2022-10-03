using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Language;

public class GetLanguageRequest : IRequest<GetLanguageResponse>
{
    public GetLanguageRequest(string cultureCode)
    {
        CultureCode = cultureCode;
    }

    public string CultureCode { get; set; }
}
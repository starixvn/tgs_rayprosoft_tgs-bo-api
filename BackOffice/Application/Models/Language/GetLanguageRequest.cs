using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Language;

public class GetLanguageRequest(string cultureCode) : IRequest<GetLanguageResponse>
{
    public string CultureCode { get; set; } = cultureCode;
}

using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Language;

public class GetLanguageResponse : MessageResponse
{
    public LanguageModel Language { get; set; }
}

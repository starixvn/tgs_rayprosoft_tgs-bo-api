namespace TGS.Services.BackOffice.Application.Models.Language;

public class LanguageRequest : BaseRequest
{
    public string CultureCode { get; set; }
    public string LanguageCode { get; set; }
    public string Name { get; set; }
    public string LocalizedName { get; set; }
}
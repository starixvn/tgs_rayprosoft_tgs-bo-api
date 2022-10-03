using System;

namespace TGS.Services.BackOffice.Application.Models.Language;

public class LanguageModel
{
    public string CultureCode { get; set; }
    public string LanguageCode { get; set; }
    public string Name { get; set; }
    public string LocalizedName { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public string CreatedByUsername { get; set; }
    public string UpdatedByUsername { get; set; }
}
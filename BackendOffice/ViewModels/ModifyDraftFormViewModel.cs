using System.Collections.Generic;

namespace TGS.Apis.BackendOffice.ViewModels;

public class ModifyDraftFormViewModel
{
    public required string Description { get; set; }

    public required IReadOnlyDictionary<string, ArticleViewModel> LocalizedArticles { get; set; }
}

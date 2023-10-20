using System;
using System.Collections.Generic;

namespace TGS.Apis.BackendOffice.ViewModels;

public class CreateDraftFormViewModel
{
    public required string Description { get; set; }

    public required IReadOnlyDictionary<string, ArticleViewModel> LocalizedArticles { get; set; }

    public DateOnly? PublishDate { get; set; }
}

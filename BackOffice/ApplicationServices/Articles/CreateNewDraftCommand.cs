using System;
using System.Collections.Generic;
using Tgs.ArticleManagement.Clients;

namespace TGS.Services.BackOffice.ApplicationServices.Articles;
public record CreateNewDraftCommand(
    Guid BrandId,
    string Description,
    DateOnly? PublishDate,
    IReadOnlyDictionary<string, Article> LocalizedArticles,
    string Modifier);

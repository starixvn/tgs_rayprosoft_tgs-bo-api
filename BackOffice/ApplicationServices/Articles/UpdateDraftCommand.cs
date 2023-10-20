using System;
using Tgs.ArticleManagement.Clients;

namespace TGS.Services.BackOffice.ApplicationServices.Articles;
public record UpdateDraftCommand(Guid FormId, UpdateDraftForm UpdateDraftData);

using System;

namespace TGS.Services.BackOffice.ApplicationServices.Articles;
public record PublishArticleCommand(Guid FormId, DateOnly PublishDate);

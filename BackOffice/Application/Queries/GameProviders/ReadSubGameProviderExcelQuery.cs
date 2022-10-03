using System.Collections.Generic;
using System.IO;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

public record ReadSubGameProviderExcelQuery(Stream FileStream);

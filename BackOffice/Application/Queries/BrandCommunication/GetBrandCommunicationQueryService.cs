using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Interfaces;
using CoreModels = TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Application.Queries.BrandCommunication;

internal class GetBrandCommunicationQueryService : IQueryService<BrandCommunicationQuery, BrandCommunicationInfo>
{
    private readonly ITgsQueries<CoreModels.BrandCommunication> _tgsQueries;

    public GetBrandCommunicationQueryService(ITgsQueries<CoreModels.BrandCommunication> tgsQueries)
    {
        _tgsQueries = tgsQueries ?? throw new ArgumentNullException(nameof(tgsQueries));
    }

    public IAsyncEnumerable<BrandCommunicationInfo> QueryAsync(
        BrandCommunicationQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = _tgsQueries.GetQueries()
            .Where(x => x.BrandId == query.BrandId)
            .ToAsyncEnumerable();

        return result.Select(x => new BrandCommunicationInfo()
        {
            Id = x.Id,
            CommunicationType = x.Type,
            Phone = x.Phone,
            AppId = x.AppId,
            QRCodeUrl = x.QRCodeUrl
        });
    }
}

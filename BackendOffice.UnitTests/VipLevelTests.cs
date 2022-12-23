using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.VipLevelService.Clients;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Tests.BackendOffice.UnitTests;

public class VipLevelTests
{
    [Test]
    public async Task Get_VipLevels_By_Brand()
    {
        var fakeVipLevelServiceClient = Substitute.For<IVipLevelServiceClient>();

        var sut = new AllVipLevelsQueryService(fakeVipLevelServiceClient);

        var query = new AllVipLevelsQuery(Guid.NewGuid());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeVipLevelServiceClient.Received(1)
            .GetBrandAllVipLevelsAsync(Arg.Is(query.BrandId));
    }
}

using NSubstitute;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AgentQueryTests
{
    [Test]
    public async Task Query_All_AgentInfo_By_Brand()
    {
        var fakeAgentRepository = Substitute.For<IAgentRepository>();

        var sut = new AllAgentsByBrandQueryService(fakeAgentRepository);

        var query = new AllAgentsByBrandQuery(Guid.NewGuid());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakeAgentRepository.Received(1)
            .GetAgentsAsync(Arg.Is(query.BrandId));
    }
}

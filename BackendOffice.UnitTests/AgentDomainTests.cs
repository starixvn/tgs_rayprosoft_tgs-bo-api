using NSubstitute;
using Tgs.DomainSettings.Clients;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Agents;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AgentDomainTests
{
    [Test]
    public async Task Add_Domain_To_A_Agent()
    {
        var moqAgentRepository = Substitute.For<IAgentRepository>();

        var sut = new AddAgentDomainCommandService(moqAgentRepository);

        var command = new AddAgentDomainCommand(
            Guid.NewGuid(),
            "test.com");

        await sut.ExecuteAsync(command);

        _ = moqAgentRepository.Received(1).AddDomainToAgentAsync(
            Arg.Is<Guid>(x => x == command.AgentId),
            Arg.Is<string>(x => x == command.DomainName));
    }

    [Test]
    public async Task Apply_Ingress_For_AgentDomain()
    {
        var moqDomainSettingClient = Substitute.For<IDomainSettingClient>();

        var sut = new ApplyAgentDomainIngressCommandService(moqDomainSettingClient);

        var command = new ApplyAgentDomainIngressCommand(Guid.NewGuid());

        await sut.ExecuteAsync(command);

        _ = moqDomainSettingClient.Received(1).ApplyAgentDomainIngressAsync(Arg.Is<Guid>(x => x == command.AgentId));
    }

    [Test]
    public async Task Create_Dns_Zone_For_AgentDomain()
    {
        var moqDomainSettingClient = Substitute.For<IDomainSettingClient>();

        var sut = new CreateAgentDnsZoneCommandService(moqDomainSettingClient);

        var command = new CreateAgentDnsZoneCommand(Guid.NewGuid());

        await sut.ExecuteAsync(command);

        _ = moqDomainSettingClient.Received(1).CreateZoneAsync(Arg.Is<Guid>(x => x == command.AgentId));
    }

    [Test]
    public async Task Delete_Dns_Zone_For_AgentDomain()
    {
        var moqDomainSettingClient = Substitute.For<IDomainSettingClient>();

        var sut = new DeleteAgentDnsZoneCommandService(moqDomainSettingClient);

        var command = new DeleteAgentDnsZoneCommand(Guid.NewGuid());

        await sut.ExecuteAsync(command);

        _ = moqDomainSettingClient.Received(1).DeleteZoneAsync(Arg.Is<Guid>(x => x == command.AgentId));
    }

    [Test]
    public async Task Delete_Ingress_For_AgentDomain()
    {
        var moqDomainSettingClient = Substitute.For<IDomainSettingClient>();

        var sut = new DeleteAgentDomainIngressCommandService(moqDomainSettingClient);

        var command = new DeleteAgentDomainIngressCommand(Guid.NewGuid());

        await sut.ExecuteAsync(command);

        _ = moqDomainSettingClient.DeleteAgentDomainIngressAsync(Arg.Is<Guid>(x => x == command.AgentId));
    }

    [Test]
    public async Task Get_Domains_From_A_Agent()
    {
        var moqAgentRepository = Substitute.For<IAgentRepository>();

        var sut = new AgentDomainsQueryService(moqAgentRepository);

        var query = new AgentDomainsQuery(Guid.NewGuid());

        _ = moqAgentRepository.QueryDomainsFromAgentAsync(Arg.Any<Guid>())
            .Returns(Array.Empty<string>().ToAsyncEnumerable());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = moqAgentRepository.Received(1).QueryDomainsFromAgentAsync(Arg.Is<Guid>(x => x == query.AgentId));
    }

    [Test]
    public async Task Remove_Domain_From_A_Agent()
    {
        var moqAgentRepository = Substitute.For<IAgentRepository>();

        var sut = new RemoveAgentDomainCommandService(moqAgentRepository);

        var command = new RemoveAgentDomainCommand(
            Guid.NewGuid(),
            "test.com");

        await sut.ExecuteAsync(command);

        _ = moqAgentRepository.Received(1).RemoveDomainFromAgentAsync(
            Arg.Is<Guid>(x => x == command.AgentId),
            Arg.Is<string>(x => x == command.DomainName));
    }
}